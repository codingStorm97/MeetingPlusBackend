using AiTodoApp.Application.DTOs;
using AiTodoApp.Application.Interfaces;
using MediatR;

namespace AiTodoApp.Application.Commands.SemanticSearch;

public sealed record SemanticSearchCommand(string Text, int? TopK)
    : IRequest<SemanticSearchResponseDto>, IAppScopeRequest
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserFolderPath { get; set; } = string.Empty;
}
