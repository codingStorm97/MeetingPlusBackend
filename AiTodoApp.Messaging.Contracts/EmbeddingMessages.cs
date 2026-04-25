namespace AiTodoApp.Messaging.Contracts;

public record FileUploadedNotification(
    Guid JobId,
    Guid UserId,
    string UserName,
    string FileName,
    string RelativePath,
    DateTime TimeUtc
);

public record EmbeddingJobRequestedMessage(
    Guid JobId,
    Guid UserId,
    string UserName,
    string FileName,
    string RelativePath,
    DateTime RequestedAtUtc
);

public record JobStatusChangedMessage(
    Guid JobId,
    Guid UserId,
    string FileName,
    string Status,
    DateTime TimeUtc,
    string? Error
);

public record UserNotificationMessage(
    Guid UserId,
    string EventType,
    string PayloadJson,
    DateTime TimeUtc
);
