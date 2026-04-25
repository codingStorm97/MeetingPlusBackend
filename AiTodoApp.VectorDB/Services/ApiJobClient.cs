using System.Net.Http.Json;
using AiTodoApp.Messaging.Contracts;

namespace AiTodoApp.VectorDB.Services;

public class ApiJobClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public ApiJobClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task UpdateStatusAsync(JobStatusChangedMessage message, CancellationToken cancellationToken)
    {
        var baseUrl = _configuration["Api:BaseUrl"] ?? "https://localhost:9011";
        var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/jobs/status", message, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
