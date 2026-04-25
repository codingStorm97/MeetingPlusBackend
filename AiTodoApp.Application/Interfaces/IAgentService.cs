using AiTodoApp.Application.DTOs;

namespace AiTodoApp.Application.Interfaces;

public interface IAgentService
{
    Task<ConversationAnswerDto> HandleAsync(
        Guid conversationId,
        string userId,
        string question,
        string context,
        CancellationToken cancellationToken);
}
