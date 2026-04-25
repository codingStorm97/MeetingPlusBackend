using AiTodoApp.Application.DTOs;
using AiTodoApp.Application.Interfaces;
using MediatR;

namespace AiTodoApp.Application.Queries.AskConversation;

public sealed record AskConversationQuery(
    Guid ConversationId,
    string Question,
    string Context) : IRequest<ConversationAnswerDto>, IAppScopeRequest
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserFolderPath { get; set; } = string.Empty;
}
