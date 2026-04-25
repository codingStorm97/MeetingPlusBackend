using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace AiTodoApp.HuggingFaceMcp.Infrastructure;

public interface IHuggingFaceContext
{
    Task<string> SendConversationQueryAsync(string conversationId, string question, string context, CancellationToken cancellationToken);
}

public sealed class HuggingFaceContext(HttpClient httpClient, IConfiguration configuration) : IHuggingFaceContext
{
    public async Task<string> SendConversationQueryAsync(
        string conversationId,
        string question,
        string context,
        CancellationToken cancellationToken)
    {
        var apiKey = configuration["HuggingFace:ApiKey"] ?? configuration["HF_API_KEY"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("HuggingFace API key missing (HuggingFace:ApiKey or HF_API_KEY).");
        }

        var model = configuration["HuggingFace:Model"] ?? "MiniMaxAI/MiniMax-M2.7:novita";
        var endpoint = configuration["HuggingFace:Endpoint"] ?? "https://router.huggingface.co/v1/chat/completions";
        var maxNewTokens = int.TryParse(configuration["HuggingFace:MaxNewTokens"], out var parsedMaxTokens)
            ? parsedMaxTokens
            : 256;

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = JsonContent.Create(new
        {
            model,
            stream = false,
            messages = new object[]
            {
                new
                {
                    role = "user",
                    content = BuildPrompt(conversationId, question, context)
                }
            },
            max_tokens = maxNewTokens
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var raw = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"HuggingFace request failed: {(int)response.StatusCode} {response.StatusCode}. {raw}");
        }

        return ExtractChatCompletion(raw);
    }

    private static string BuildPrompt(string conversationId, string question, string context)
    {
        if (string.IsNullOrWhiteSpace(context))
        {
            return $"ConversationId: {conversationId}\nQuestion:\n{question}";
        }

        return $"ConversationId: {conversationId}\nContext:\n{context}\n\nQuestion:\n{question}";
    }

    private static string ExtractChatCompletion(string rawJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(rawJson);
            var root = doc.RootElement;
            if (root.TryGetProperty("choices", out var choices)
                && choices.ValueKind == JsonValueKind.Array
                && choices.GetArrayLength() > 0)
            {
                var first = choices[0];
                if (first.TryGetProperty("message", out var message)
                    && message.TryGetProperty("content", out var content))
                {
                    var text = content.GetString();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        return text.Trim();
                    }
                }
            }
        }
        catch
        {
            // Fall back to raw payload for troubleshooting.
        }

        return rawJson;
    }
}
