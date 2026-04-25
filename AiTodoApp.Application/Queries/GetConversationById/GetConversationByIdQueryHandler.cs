using AiTodoApp.Application.DTOs;
using AiTodoApp.Application.Interfaces;
using MediatR;

namespace AiTodoApp.Application.Queries.GetConversationById;

public sealed class GetConversationByIdQueryHandler(IConversationRepository conversationRepository)
    : IRequestHandler<GetConversationByIdQuery, ConversationThreadDto?>
{
    public async Task<ConversationThreadDto?> Handle(GetConversationByIdQuery request, CancellationToken cancellationToken)
    {
        var conversation = await conversationRepository.GetByIdForUserAsync(
            request.ConversationId,
            request.UserId,
            cancellationToken);

        if (conversation is null)
        {
            return null;
        }

        return new ConversationThreadDto(
            conversation.Id,
            conversation.Title,
            conversation.CreatedAt,
            conversation.Questions
                .OrderBy(q => q.CreatedAt)
                .Select(q => new ConversationQuestionDto(
                    q.QuestionId,
                    q.Question,
                    q.Answer,
                    q.CreatedAt,
                    q.AnsweredTime))
                .ToList());
    }
}
