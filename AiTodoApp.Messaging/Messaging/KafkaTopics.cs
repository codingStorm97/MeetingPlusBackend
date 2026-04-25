namespace AiTodoApp.Messaging.Messaging;

public static class KafkaTopics
{
    public const string FileUploadedConfigKey = "Kafka:Topics:FileUploaded";
    public const string EmbeddingJobRequestedConfigKey = "Kafka:Topics:EmbeddingJobRequested";
    public const string JobStatusChangedConfigKey = "Kafka:Topics:JobStatusChanged";
    public const string UserNotificationsConfigKey = "Kafka:Topics:UserNotifications";

    public static string FileUploaded(IConfiguration configuration) =>
        configuration[FileUploadedConfigKey] ?? "file-uploaded";

    public static string EmbeddingJobRequested(IConfiguration configuration) =>
        configuration[EmbeddingJobRequestedConfigKey] ?? "embedding-job-requested";

    public static string JobStatusChanged(IConfiguration configuration) =>
        configuration[JobStatusChangedConfigKey] ?? "job-status-changed";

    public static string UserNotifications(IConfiguration configuration) =>
        configuration[UserNotificationsConfigKey] ?? "user-notifications";
}
