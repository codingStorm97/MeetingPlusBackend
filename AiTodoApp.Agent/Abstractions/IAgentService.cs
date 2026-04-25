using AiTodoApp.Application.DTOs;

namespace AiTodoApp.Agent.Abstractions;

public interface IAgentService
{
    Task<ConversationAnswerDto> HandleAsync(
        Guid conversationId,
        string userId,
        string question,
        string context,
        CancellationToken cancellationToken);
}
