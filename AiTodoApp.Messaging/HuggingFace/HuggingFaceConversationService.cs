using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using AiTodoApp.Application.DTOs;
using AiTodoApp.Application.Interfaces;

namespace AiTodoApp.Messaging.HuggingFace;

public sealed class HuggingFaceConversationService(HttpClient httpClient, IConfiguration configuration)
    : IConversationService
{
    public async Task<string> GetVectorSearchQuery(
        Guid conversationId,
        string question,
        CancellationToken cancellationToken = default)
    {
        var keywordPrompt = "Extract the best vector-search keywords for this question. " +
                            "Return ONLY valid JSON with this exact schema: " +
                            "{\"vectorQuery\":\"string\",\"keywords\":[\"string\"]}. " +
                            "Do not include markdown, code blocks, or extra text.";

        var response = await AskAsync(
            conversationId,
            keywordPrompt,
            $"user_question: {question}",
            cancellationToken);

        return TryExtractVectorQuery(response.Answer, question);
    }

    public async Task<ConversationAnswerDto> AskAsync(
        Guid conversationId,
        string question,
        string context,
        CancellationToken cancellationToken = default)
    {

        var baseUrl = configuration["HuggingFaceMcp:BaseUrl"] ?? "http://localhost:9090";
        var endpoint = $"{baseUrl.TrimEnd('/')}/api/conversation/query";

        using var response = await httpClient.PostAsJsonAsync(
            endpoint,
            new
            {
                conversationId,
                question,
                context
            },
            cancellationToken);

        var payload = await response.Content.ReadFromJsonAsync<ConversationMcpResponse>(cancellationToken: cancellationToken);
        if (!response.IsSuccessStatusCode || payload is null)
        {
            var raw = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Conversation service failed: {(int)response.StatusCode} {response.StatusCode}. {raw}");
        }

        return new ConversationAnswerDto(
            payload.ConversationId,
            Guid.Empty,
            question,
            payload.Result ?? string.Empty,
            "answer_question",
            null);
    }

    private static string TryExtractVectorQuery(string rawAnswer, string fallbackQuestion)
    {
        if (string.IsNullOrWhiteSpace(rawAnswer))
        {
            return fallbackQuestion;
        }

        try
        {
            var parsed = JsonSerializer.Deserialize<VectorQueryPayload>(rawAnswer);
            if (!string.IsNullOrWhiteSpace(parsed?.VectorQuery))
            {
                return parsed.VectorQuery.Trim();
            }
        }
        catch (JsonException)
        {
            // Best-effort fallback below for cases where model adds surrounding text.
        }

        var jsonMatch = Regex.Match(rawAnswer, @"\{[\s\S]*\}");
        if (jsonMatch.Success)
        {
            try
            {
                var parsed = JsonSerializer.Deserialize<VectorQueryPayload>(jsonMatch.Value);
                if (!string.IsNullOrWhiteSpace(parsed?.VectorQuery))
                {
                    return parsed.VectorQuery.Trim();
                }
            }
            catch (JsonException)
            {
                // Ignore and use fallback.
            }
        }

        return fallbackQuestion;
    }

    private sealed record ConversationMcpResponse(Guid ConversationId, string? Result);

    private sealed class VectorQueryPayload
    {
        [JsonPropertyName("vectorQuery")]
        public string? VectorQuery { get; init; }
    }
}
