using AiTodoApp.Application.DTOs;
using AiTodoApp.Application.Interfaces;
using MediatR;

namespace AiTodoApp.Application.Commands.SemanticSearch;

public sealed class SemanticSearchCommandHandler(ISemanticSearchService semanticSearchService)
    : IRequestHandler<SemanticSearchCommand, SemanticSearchResponseDto>
{
    public async Task<SemanticSearchResponseDto> Handle(
        SemanticSearchCommand request,
        CancellationToken cancellationToken)
    {
        return await semanticSearchService.SearchAsync(
            request.UserId,
            request.Text,
            request.TopK ?? 5,
            cancellationToken);
    }
}
