using System.Text.Json;
using AiTodoApp.Messaging.Contracts;
using AiTodoApp.Messaging.Messaging.Common;
using Confluent.Kafka;

namespace AiTodoApp.Messaging.Messaging.Consumers;

public class JobStatusChangedConsumer : BackgroundService
{
    private readonly KafkaConnectionFactory _connectionFactory;
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<JobStatusChangedConsumer> _logger;

    public JobStatusChangedConsumer(
        KafkaConnectionFactory connectionFactory,
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory,
        ILogger<JobStatusChangedConsumer> logger)
    {
        _connectionFactory = connectionFactory;
        _configuration = configuration;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = _connectionFactory.CreateConsumerConfig(
            "Kafka:Consumers:JobStatusChanged:GroupId",
            "messaging-job-status");

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        var topic = KafkaTopics.JobStatusChanged(_configuration);
        consumer.Subscribe(topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(stoppingToken);
                var message = JsonSerializer.Deserialize<JobStatusChangedMessage>(result.Message.Value);
                if (message is null)
                {
                    continue;
                }

                using var scope = _scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<JobStatusChangedMessageHandler>();
                await handler.HandleAsync(message, stoppingToken);
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
