using AiTodoApp.Application.Interfaces;
using AiTodoApp.Domain.Entities;
using AiTodoApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AiTodoApp.Infrastructure.Repositories;

public sealed class ConversationRepository(AppDbContext dbContext) : IConversationRepository
{
    public async Task AddAsync(Conversation conversation, CancellationToken cancellationToken = default)
    {
        await dbContext.Conversations.AddAsync(conversation, cancellationToken);
    }

    public async Task<IReadOnlyList<Conversation>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Conversations
            .Where(x => x.UserId == userId)
            .Include(x => x.Questions.OrderBy(q => q.CreatedAt))
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Conversation?> GetByIdForUserAsync(
        Guid conversationId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Conversations
            .Include(x => x.Questions.OrderBy(q => q.CreatedAt))
            .FirstOrDefaultAsync(x => x.Id == conversationId && x.UserId == userId, cancellationToken);
    }

    public async Task<bool> ExistsForUserAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Conversations
            .AnyAsync(x => x.Id == conversationId && x.UserId == userId, cancellationToken);
    }

    public async Task AddQuestionAsync(ConversationQuestion question, CancellationToken cancellationToken = default)
    {
        await dbContext.ConversationQuestions.AddAsync(question, cancellationToken);
    }

    public async Task UpdateAnswerAsync(Guid questionId, string answer, DateTime answeredTime, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.ConversationQuestions
            .FirstOrDefaultAsync(x => x.QuestionId == questionId, cancellationToken);
        if (entity is null)
        {
            return;
        }

        entity.Answer = answer;
        entity.AnsweredTime = answeredTime;
    }
}
