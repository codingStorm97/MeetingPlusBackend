using AiTodoApp.Application.DTOs;

namespace AiTodoApp.Application.Interfaces;

public interface IConversationService
{
    Task<ConversationAnswerDto> AskAsync(
        Guid conversationId,
        string question,
        string context,
        CancellationToken cancellationToken = default);

    Task<string> GetVectorSearchQuery(
        Guid conversationId,
        string question,
        CancellationToken cancellationToken = default);
}
