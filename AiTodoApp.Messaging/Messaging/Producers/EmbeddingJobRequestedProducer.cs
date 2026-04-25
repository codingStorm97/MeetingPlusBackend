using System.Text.Json;
using AiTodoApp.Messaging.Contracts;
using Confluent.Kafka;

namespace AiTodoApp.Messaging.Messaging.Producers;

public class EmbeddingJobRequestedProducer
{
    private readonly IProducer<string, string> _producer;
    private readonly IConfiguration _configuration;

    public EmbeddingJobRequestedProducer(IProducer<string, string> producer, IConfiguration configuration)
    {
        _producer = producer;
        _configuration = configuration;
    }

    public Task PublishAsync(EmbeddingJobRequestedMessage message, CancellationToken cancellationToken)
    {
        return _producer.ProduceAsync(
            KafkaTopics.EmbeddingJobRequested(_configuration),
            new Message<string, string>
            {
                Key = message.UserId.ToString(),
                Value = JsonSerializer.Serialize(message)
            },
            cancellationToken);
    }
}
