using Microsoft.Extensions.Configuration;

namespace AiTodoApp.Infrastructure.Messaging;

public static class KafkaTopics
{
    public static string FileUploaded(IConfiguration configuration) =>
        configuration["Kafka:Topics:FileUploaded"] ?? "file-uploaded";

    public static string JobStatusChanged(IConfiguration configuration) =>
        configuration["Kafka:Topics:JobStatusChanged"] ?? "job-status-changed";
}
