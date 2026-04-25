namespace AiTodoApp.VectorDB.Services;

public class TextChunker
{
    public IReadOnlyList<string> Chunk(string text, int chunkSize, int overlap)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Array.Empty<string>();
        }

        var normalized = text.Replace("\r\n", "\n").Trim();
        var words = normalized
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

        if (words.Length == 0)
        {
            return Array.Empty<string>();
        }

        var chunks = new List<string>();
        var start = 0;
        var effectiveChunkSize = Math.Max(1, chunkSize);
        var step = Math.Max(1, effectiveChunkSize - Math.Max(0, overlap));

        while (start < words.Length)
        {
            var length = Math.Min(effectiveChunkSize, words.Length - start);
            var chunk = string.Join(' ', words.AsSpan(start, length).ToArray()).Trim();
            if (!string.IsNullOrWhiteSpace(chunk))
            {
                chunks.Add(chunk);
            }

            if (start + length >= words.Length)
            {
                break;
            }

            start += step;
        }

        return chunks;
    }
}
