using AiTodoApp.Application.DTOs;
using AiTodoApp.Application.Interfaces;
using MediatR;

namespace AiTodoApp.Application.Commands.CreateConversation;

public sealed record CreateConversationCommand(string? Title) : IRequest<ConversationThreadDto>, IAppScopeRequest
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserFolderPath { get; set; } = string.Empty;
}
