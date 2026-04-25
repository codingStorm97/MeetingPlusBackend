using AiTodoApp.Application.DTOs;
using AiTodoApp.Application.Interfaces;
using MediatR;

namespace AiTodoApp.Application.Queries.GetUserConversations;

public sealed record GetUserConversationsQuery : IRequest<IReadOnlyList<ConversationDto>>, IAppScopeRequest
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserFolderPath { get; set; } = string.Empty;
}
