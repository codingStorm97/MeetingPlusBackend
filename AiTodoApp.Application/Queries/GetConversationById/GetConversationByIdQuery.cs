using AiTodoApp.Application.DTOs;
using AiTodoApp.Application.Interfaces;
using MediatR;

namespace AiTodoApp.Application.Queries.GetConversationById;

public sealed record GetConversationByIdQuery(Guid ConversationId) : IRequest<ConversationThreadDto?>, IAppScopeRequest
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserFolderPath { get; set; } = string.Empty;
}
