namespace AiTodoApp.Application.DTOs;

public record FileUploadResultDto(
    Guid JobId,
    Guid UserId,
    string UserName,
    string FileName,
    string Message,
    string Path,
    DateTime TimeUtc
);
