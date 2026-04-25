using AiTodoApp.Application.Interfaces;
using Confluent.Kafka;
using System.Text.Json;
using AiTodoApp.Messaging.Contracts;
using Microsoft.Extensions.Configuration;

namespace AiTodoApp.Infrastructure.Messaging;

public class MessagingPublisher : IMessagingPublisher
{
    private readonly IProducer<string, string> _producer;

    public MessagingPublisher(IConfiguration configuration)
    {
        _configuration = configuration;
        var bootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9050";
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = bootstrapServers
        };

        _producer = new ProducerBuilder<string, string>(producerConfig).Build();
    }

    public async Task PublishFileUploadedAsync(FileUploadedNotification notification, CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(notification);
        await _producer.ProduceAsync(
            KafkaTopics.FileUploaded(_configuration),
            new Message<string, string>
            {
                Key = notification.UserId.ToString(),
                Value = payload
            },
            cancellationToken);
    }

    public async Task PublishJobStatusChangedAsync(JobStatusChangedMessage message, CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(message);
        await _producer.ProduceAsync(
            KafkaTopics.JobStatusChanged(_configuration),
            new Message<string, string>
            {
                Key = message.UserId.ToString(),
                Value = payload
            },
            cancellationToken);
    }

    private readonly IConfiguration _configuration;
}
