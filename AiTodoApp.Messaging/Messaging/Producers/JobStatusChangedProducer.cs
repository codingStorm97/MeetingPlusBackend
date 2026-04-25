using System.Text.Json;
using AiTodoApp.Messaging.Contracts;
using AiTodoApp.VectorDB.Services;
using Confluent.Kafka;

namespace AiTodoApp.Messaging.Messaging.Producers;

public class JobStatusChangedProducer : IJobStatusMessageProducer
{
    private readonly IProducer<string, string> _producer;
    private readonly IConfiguration _configuration;

    public JobStatusChangedProducer(IProducer<string, string> producer, IConfiguration configuration)
    {
        _producer = producer;
        _configuration = configuration;
    }

    public Task PublishStatusAsync(JobStatusChangedMessage message, CancellationToken cancellationToken)
    {
        var topic = KafkaTopics.JobStatusChanged(_configuration);
        return _producer.ProduceAsync(
            topic,
            new Message<string, string>
            {
                Key = message.UserId.ToString(),
                Value = JsonSerializer.Serialize(message)
            },
            cancellationToken);
    }
}
