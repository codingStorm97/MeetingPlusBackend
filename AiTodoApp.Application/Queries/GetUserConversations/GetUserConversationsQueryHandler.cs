using AiTodoApp.Application.DTOs;
using AiTodoApp.Application.Interfaces;
using MediatR;

namespace AiTodoApp.Application.Queries.GetUserConversations;

public sealed class GetUserConversationsQueryHandler(IConversationRepository conversationRepository)
    : IRequestHandler<GetUserConversationsQuery, IReadOnlyList<ConversationDto>>
{
    public async Task<IReadOnlyList<ConversationDto>> Handle(
        GetUserConversationsQuery request,
        CancellationToken cancellationToken)
    {
        var conversations = await conversationRepository.GetByUserAsync(request.UserId, cancellationToken);
        return conversations
            .Select(x =>
            {
                var firstQuestion = x.Questions.OrderBy(q => q.CreatedAt).FirstOrDefault();
                var createdAt = firstQuestion?.CreatedAt ?? x.CreatedAt;
                var conversationName = BuildConversationName(firstQuestion?.Question);
                return new ConversationDto(x.Id, conversationName, createdAt);
            })
            .OrderByDescending(x => x.CreatedAt)
            .ToList();
    }

    private static string BuildConversationName(string? firstQuestion)
    {
        if (string.IsNullOrWhiteSpace(firstQuestion))
        {
            return "New conversation";
        }

        var words = firstQuestion
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Take(5)
            .ToArray();

        return words.Length == 0 ? "New conversation" : string.Join(' ', words);
    }
}
