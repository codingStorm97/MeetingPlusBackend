using Confluent.Kafka;

namespace AiTodoApp.Messaging.Messaging.Common;

public class KafkaConnectionFactory
{
    private readonly IConfiguration _configuration;

    public KafkaConnectionFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public ProducerConfig CreateProducerConfig()
    {
        return new ProducerConfig
        {
            BootstrapServers = GetBootstrapServers()
        };
    }

    public ConsumerConfig CreateConsumerConfig(string groupIdConfigPath, string defaultGroupId)
    {
        return new ConsumerConfig
        {
            BootstrapServers = GetBootstrapServers(),
            GroupId = _configuration[groupIdConfigPath] ?? defaultGroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };
    }

    private string GetBootstrapServers()
    {
        return _configuration["Kafka:BootstrapServers"] ?? "localhost:9050";
    }
}
