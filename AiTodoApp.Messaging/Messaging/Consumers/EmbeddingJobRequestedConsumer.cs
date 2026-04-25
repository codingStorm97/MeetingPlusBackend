using System.Text.Json;
using AiTodoApp.Messaging.Contracts;
using AiTodoApp.Messaging.Messaging.Common;
using AiTodoApp.VectorDB.Services;
using Confluent.Kafka;

namespace AiTodoApp.Messaging.Messaging.Consumers;

public class EmbeddingJobRequestedConsumer : BackgroundService
{
    private readonly KafkaConnectionFactory _connectionFactory;
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EmbeddingJobRequestedConsumer> _logger;

    public EmbeddingJobRequestedConsumer(
        KafkaConnectionFactory connectionFactory,
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory,
        ILogger<EmbeddingJobRequestedConsumer> logger)
    {
        _connectionFactory = connectionFactory;
        _configuration = configuration;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = _connectionFactory.CreateConsumerConfig(
            "Kafka:Consumers:EmbeddingJobRequested:GroupId",
            "messaging-embedding-worker");

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        var topic = KafkaTopics.EmbeddingJobRequested(_configuration);
        consumer.Subscribe(topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(stoppingToken);
                var message = JsonSerializer.Deserialize<EmbeddingJobRequestedMessage>(result.Message.Value);
                if (message is null)
                {
                    continue;
                }

                using var scope = _scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<EmbeddingJobProcessor>();
                await processor.ProcessAsync(message, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed consuming topic {Topic}", topic);
            }
        }
    }
}
