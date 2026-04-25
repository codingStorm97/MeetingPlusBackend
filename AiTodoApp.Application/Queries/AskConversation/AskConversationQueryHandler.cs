using AiTodoApp.Application.DTOs;
using AiTodoApp.Application.Interfaces;
using AiTodoApp.Domain.Entities;
using MediatR;

namespace AiTodoApp.Application.Queries.AskConversation;

public sealed class AskConversationQueryHandler(
    IConversationRepository conversationRepository,
    IUnitOfWork unitOfWork,
    IAgentService agentService) : IRequestHandler<AskConversationQuery, ConversationAnswerDto>
{
    public async Task<ConversationAnswerDto> Handle(AskConversationQuery request, CancellationToken cancellationToken)
    {
        var exists = await conversationRepository.ExistsForUserAsync(
            request.ConversationId,
            request.UserId,
            cancellationToken);

        if (!exists)
        {
            throw new InvalidOperationException("Conversation not found for current user.");
        }

        var questionEntity = new ConversationQuestion
        {
            QuestionId = Guid.NewGuid(),
            ConversationId = request.ConversationId,
            UserId = request.UserId,
            Question = request.Question,
            Answer = null,
            CreatedAt = DateTime.UtcNow
        };

        await conversationRepository.AddQuestionAsync(questionEntity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var response = await agentService.HandleAsync(
            request.ConversationId,
            request.UserId.ToString(),
            request.Question,
            request.Context,
            cancellationToken);

        await conversationRepository.UpdateAnswerAsync(
            questionEntity.QuestionId,
            response.Answer,
            DateTime.UtcNow,
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return response with { QuestionId = questionEntity.QuestionId };
    }
}
