using System.Text.Json;
using AiTodoApp.Messaging.Contracts;
using Confluent.Kafka;

namespace AiTodoApp.Messaging.Messaging.Producers;

public class UserNotificationProducer
{
    private readonly IProducer<string, string> _producer;
    private readonly IConfiguration _configuration;

    public UserNotificationProducer(IProducer<string, string> producer, IConfiguration configuration)
    {
        _producer = producer;
        _configuration = configuration;
    }

    public Task PublishFileUploadedAsync(FileUploadedNotification message, CancellationToken cancellationToken)
    {
        return PublishAsync(
            new UserNotificationMessage(
                message.UserId,
                "fileUploaded",
                JsonSerializer.Serialize(message),
                DateTime.UtcNow),
            cancellationToken);
    }

    public Task PublishJobStatusChangedAsync(JobStatusChangedMessage message, CancellationToken cancellationToken)
    {
        return PublishAsync(
            new UserNotificationMessage(
                message.UserId,
                "jobStatusChanged",
                JsonSerializer.Serialize(message),
                DateTime.UtcNow),
            cancellationToken);
    }

    private Task PublishAsync(UserNotificationMessage message, CancellationToken cancellationToken)
    {
        return _producer.ProduceAsync(
            KafkaTopics.UserNotifications(_configuration),
            new Message<string, string>
            {
                Key = message.UserId.ToString(),
                Value = JsonSerializer.Serialize(message)
            },
            cancellationToken);
    }
}
