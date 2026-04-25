namespace AiTodoApp.Application.DTOs;

public record FileDownloadResultDto(
    string FileName,
    byte[] Content
);
