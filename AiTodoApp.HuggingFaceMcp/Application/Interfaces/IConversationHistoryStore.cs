namespace AiTodoApp.HuggingFaceMcp.Application.Interfaces;

public interface IConversationHistoryStore
{
    Task AppendAsync(
        string conversationId,
        string question,
        string answer,
        CancellationToken cancellationToken);
}
