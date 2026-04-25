using System.Net.Http.Json;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AiTodoApp.VectorDB.Services;

public class QdrantService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public QdrantService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task UpsertChunksAsync(
        Guid userId,
        Guid jobId,
        string fileName,
        string relativePath,
        IReadOnlyList<(int ChunkIndex, string ChunkText, IReadOnlyList<float> Vector)> chunks,
        CancellationToken cancellationToken)
    {
        var collection = $"user_{userId.ToString("N")}";
        await EnsureCollectionAsync(collection, cancellationToken);

        var points = chunks.Select(chunk => new
        {
            id = BuildDeterministicId(jobId, chunk.ChunkIndex),
            vector = chunk.Vector,
            payload = new
            {
                userId = userId.ToString(),
                jobId = jobId.ToString(),
                fileName,
                relativePath,
                chunkIndex = chunk.ChunkIndex,
                chunkText = chunk.ChunkText,
                createdAtUtc = DateTime.UtcNow
            }
        });

        var response = await _httpClient.PutAsJsonAsync(
            $"{GetBaseUrl()}/collections/{collection}/points?wait=true",
            new { points },
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<SemanticSearchChunkResult>> SearchChunksAsync(
        Guid userId,
        IReadOnlyList<float> vector,
        int limit,
        CancellationToken cancellationToken)
    {
        var collection = $"user_{userId.ToString("N")}";
        var normalizedLimit = Math.Max(1, limit);

        using var searchResponse = await _httpClient.PostAsJsonAsync(
            $"{GetBaseUrl()}/collections/{collection}/points/search",
            new
            {
                vector,
                limit = normalizedLimit,
                with_payload = true
            },
            cancellationToken);

        if (searchResponse.StatusCode == HttpStatusCode.NotFound)
        {
            return [];
        }

        if (searchResponse.IsSuccessStatusCode)
        {
            return await ParseSearchResultsAsync(searchResponse, cancellationToken);
        }

        var searchError = await searchResponse.Content.ReadAsStringAsync(cancellationToken);

        // Compatibility fallback for newer Qdrant APIs.
        using var queryResponse = await _httpClient.PostAsJsonAsync(
            $"{GetBaseUrl()}/collections/{collection}/points/query",
            new
            {
                query = vector,
                limit = normalizedLimit,
                with_payload = true
            },
            cancellationToken);

        if (queryResponse.StatusCode == HttpStatusCode.NotFound)
        {
            return [];
        }

        if (queryResponse.IsSuccessStatusCode)
        {
            return await ParseSearchResultsAsync(queryResponse, cancellationToken);
        }

        var queryError = await queryResponse.Content.ReadAsStringAsync(cancellationToken);
        throw new InvalidOperationException(
            $"Qdrant semantic search failed. search_error={searchError}; query_error={queryError}");
    }

    private async Task EnsureCollectionAsync(string collection, CancellationToken cancellationToken)
    {
        var size = int.TryParse(_configuration["Embedding:VectorSize"], out var configuredSize) ? configuredSize : 1536;
        var response = await _httpClient.PutAsJsonAsync(
            $"{GetBaseUrl()}/collections/{collection}",
            new
            {
                vectors = new
                {
                    size,
                    distance = "Cosine"
                }
            },
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    private string GetBaseUrl() =>
        _configuration["Qdrant:BaseUrl"] ?? throw new InvalidOperationException("Qdrant:BaseUrl missing.");

    private static string BuildDeterministicId(Guid jobId, int chunkIndex)
    {
        var input = $"{jobId:N}:{chunkIndex}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash[..16]).ToLowerInvariant();
    }

    private static async Task<IReadOnlyList<SemanticSearchChunkResult>> ParseSearchResultsAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var payload = await response.Content.ReadFromJsonAsync<QdrantSearchResponse>(cancellationToken: cancellationToken);
        if (payload?.Result is null)
        {
            return [];
        }

        return payload.Result
            .Select(x => new SemanticSearchChunkResult(
                x.Score,
                x.Payload?.FileName ?? string.Empty,
                x.Payload?.RelativePath ?? string.Empty,
                x.Payload?.ChunkText ?? string.Empty))
            .Where(x => !string.IsNullOrWhiteSpace(x.ChunkText))
            .ToList();
    }

    private sealed class QdrantSearchResponse
    {
        [JsonPropertyName("result")]
        public List<QdrantSearchPoint>? Result { get; set; }
    }

    private sealed class QdrantSearchPoint
    {
        [JsonPropertyName("score")]
        public double Score { get; set; }

        [JsonPropertyName("payload")]
        public QdrantSearchPayload? Payload { get; set; }
    }

    private sealed class QdrantSearchPayload
    {
        [JsonPropertyName("fileName")]
        public string? FileName { get; set; }

        [JsonPropertyName("relativePath")]
        public string? RelativePath { get; set; }

        [JsonPropertyName("chunkText")]
        public string? ChunkText { get; set; }
    }
}

public sealed record SemanticSearchChunkResult(
    double Score,
    string FileName,
    string RelativePath,
    string ChunkText);
