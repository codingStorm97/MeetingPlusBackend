namespace AiTodoApp.Application.DTOs;

public sealed record SemanticSearchResultDto(
    double Score,
    string FileName,
    string RelativePath,
    string ChunkText,
    string TextBlock);
