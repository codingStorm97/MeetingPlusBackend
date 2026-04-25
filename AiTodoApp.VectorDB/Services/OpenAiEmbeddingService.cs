using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace AiTodoApp.VectorDB.Services;

public class OpenAiEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public OpenAiEmbeddingService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<IReadOnlyList<float>> CreateEmbeddingAsync(string text, CancellationToken cancellationToken)
    {
        var baseUrl = _configuration["Ollama:BaseUrl"] ?? "http://localhost:9040";
        var model = _configuration["Ollama:EmbeddingModel"] ?? "nomic-embed-text";

        using var response = await _httpClient.PostAsJsonAsync(
            $"{baseUrl.TrimEnd('/')}/api/embeddings",
            new
            {
                model,
                prompt = text
            },
            cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<OllamaEmbeddingResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Invalid Ollama embedding response.");
        if (payload.Embedding.Count == 0)
        {
            throw new InvalidOperationException("Empty Ollama embedding response.");
        }

        return payload.Embedding;
    }

    private sealed class OllamaEmbeddingResponse
    {
        [JsonPropertyName("embedding")]
        public List<float> Embedding { get; set; } = [];
    }
}
