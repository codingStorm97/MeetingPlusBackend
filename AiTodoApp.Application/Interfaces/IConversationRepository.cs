using AiTodoApp.Domain.Entities;

namespace AiTodoApp.Application.Interfaces;

public interface IConversationRepository
{
    Task AddAsync(Conversation conversation, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Conversation>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Conversation?> GetByIdForUserAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsForUserAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken = default);
    Task AddQuestionAsync(ConversationQuestion question, CancellationToken cancellationToken = default);
    Task UpdateAnswerAsync(Guid questionId, string answer, DateTime answeredTime, CancellationToken cancellationToken = default);
}
