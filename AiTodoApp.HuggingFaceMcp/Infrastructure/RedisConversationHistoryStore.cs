using System.Text.Json;
using AiTodoApp.HuggingFaceMcp.Application.Interfaces;
using AiTodoApp.HuggingFaceMcp.Application.Models;
using StackExchange.Redis;

namespace AiTodoApp.HuggingFaceMcp.Infrastructure;

public sealed class RedisConversationHistoryStore(IConnectionMultiplexer connectionMultiplexer) : IConversationHistoryStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task AppendAsync(
        string conversationId,
        string question,
        string answer,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(conversationId))
        {
            return;
        }

        var db = connectionMultiplexer.GetDatabase();
        var key = $"conversation:{conversationId.Trim()}";
        var item = new ConversationHistoryItem(question, answer);
        var payload = JsonSerializer.Serialize(item, JsonOptions);

        await db.ListRightPushAsync(key, payload);
    }
}
