namespace AiTodoApp.Application.DTOs;

public record UserFilesResultDto(
    string UserName,
    string Directory,
    IReadOnlyList<UserFileResultItemDto> Files
);

public record UserFileResultItemDto(
    Guid FileId,
    string FileName,
    string Status,
    string RelativePath,
    string DownloadUrl
);
