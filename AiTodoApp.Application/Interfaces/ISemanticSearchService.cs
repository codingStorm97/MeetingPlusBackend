using AiTodoApp.Application.DTOs;

namespace AiTodoApp.Application.Interfaces;

public interface ISemanticSearchService
{
    Task<SemanticSearchResponseDto> SearchAsync(
        Guid userId,
        string text,
        int topK,
        CancellationToken cancellationToken);
}
