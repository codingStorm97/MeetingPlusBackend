using AiTodoApp.Application.DTOs;
using AiTodoApp.Application.Interfaces;
using AiTodoApp.VectorDB.Services;
using System.Text.RegularExpressions;

namespace AiTodoApp.Api.Services;

public sealed class SemanticSearchService(
    OpenAiEmbeddingService embeddingService,
    QdrantService qdrantService) : ISemanticSearchService
{
    private const int RetrievalCount = 10;
    private const int FinalCount = 3;
    private const int SnippetLength = 600;

    public async Task<SemanticSearchResponseDto> SearchAsync(
        Guid userId,
        string text,
        int topK,
        CancellationToken cancellationToken)
    {
        var embedding = await embeddingService.CreateEmbeddingAsync(text, cancellationToken);
        var chunks = await qdrantService.SearchChunksAsync(
            userId,
            embedding,
            Math.Max(RetrievalCount, topK),
            cancellationToken);

        var deduped = DeduplicateChunks(chunks)
            .Take(FinalCount)
            .ToList();

        var results = deduped
            .Select(x => new SemanticSearchResultDto(
                x.Score,
                x.FileName,
                x.RelativePath,
                x.ChunkText,
                $"File: {x.FileName}\nPath: {x.RelativePath}\n\n{BuildSnippet(x.ChunkText, text)}"))
            .ToList();

        return new SemanticSearchResponseDto(text, results);
    }

    private static IEnumerable<SemanticSearchChunkResult> DeduplicateChunks(
        IReadOnlyList<SemanticSearchChunkResult> chunks)
    {
        var picked = new List<SemanticSearchChunkResult>();

        foreach (var candidate in chunks.OrderByDescending(x => x.Score))
        {
            var isDuplicate = picked.Any(existing =>
                string.Equals(existing.FileName, candidate.FileName, StringComparison.OrdinalIgnoreCase)
                && Similarity(existing.ChunkText, candidate.ChunkText) >= 0.72);

            if (!isDuplicate)
            {
                picked.Add(candidate);
            }
        }

        return picked;
    }

    private static double Similarity(string left, string right)
    {
        var leftTokens = Tokenize(left);
        var rightTokens = Tokenize(right);
        if (leftTokens.Count == 0 || rightTokens.Count == 0)
        {
            return 0;
        }

        var intersection = leftTokens.Intersect(rightTokens).Count();
        var union = leftTokens.Union(rightTokens).Count();
        return union == 0 ? 0 : (double)intersection / union;
    }

    private static HashSet<string> Tokenize(string value)
    {
        return value
            .ToLowerInvariant()
            .Split(new[] { ' ', '\n', '\r', '\t', ',', '.', ':', ';', '-', '_', '/', '\\', '|', '[', ']', '(', ')', '{', '}', '!' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(x => x.Length > 2)
            .ToHashSet();
    }

    private static string BuildSnippet(string chunkText, string query)
    {
        if (string.IsNullOrWhiteSpace(chunkText))
        {
            return string.Empty;
        }

        var normalized = chunkText.Trim();
        if (normalized.Length <= 700)
        {
            return normalized;
        }

        var index = FindMatchIndex(normalized, query);
        if (index < 0)
        {
            return normalized[..Math.Min(700, normalized.Length)];
        }

        var start = Math.Max(0, index - (SnippetLength / 2));
        var length = Math.Min(SnippetLength, normalized.Length - start);
        var snippet = normalized.Substring(start, length);

        if (start > 0)
        {
            snippet = $"...{snippet}";
        }

        if (start + length < normalized.Length)
        {
            snippet = $"{snippet}...";
        }

        return snippet;
    }

    private static int FindMatchIndex(string text, string query)
    {
        foreach (var term in Regex.Split(query.ToLowerInvariant(), @"\W+").Where(t => t.Length > 2))
        {
            var idx = text.ToLowerInvariant().IndexOf(term, StringComparison.Ordinal);
            if (idx >= 0)
            {
                return idx;
            }
        }

        return -1;
    }
}
