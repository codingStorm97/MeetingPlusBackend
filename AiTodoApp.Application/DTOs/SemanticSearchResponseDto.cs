namespace AiTodoApp.Application.DTOs;

public sealed record SemanticSearchResponseDto(
    string Query,
    IReadOnlyList<SemanticSearchResultDto> Results);
