namespace AiTodoApp.Application.DTOs;

public record JobItemDto(
    Guid JobId,
    Guid UserId,
    string Status,
    DateTime TimeUtc,
    Guid UploadFileId,
    string UploadFileName,
    string RelativePath
);
