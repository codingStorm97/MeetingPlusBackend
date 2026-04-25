namespace AiTodoApp.Application.DTOs;

public record CleanUserFolderResultDto(
    Guid UserId,
    string UserName,
    int DeletedFileCount,
    int DeletedJobRecordCount,
    int DeletedFileHistoryRecordCount
);
