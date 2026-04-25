using System.Text.Json;
using AiTodoApp.Messaging.Contracts;
using AiTodoApp.Messaging.Messaging.Common;
using Confluent.Kafka;

namespace AiTodoApp.Messaging.Messaging.Consumers;

public class FileUploadedConsumer : BackgroundService
{
    private readonly KafkaConnectionFactory _connectionFactory;
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<FileUploadedConsumer> _logger;

    public FileUploadedConsumer(
        KafkaConnectionFactory connectionFactory,
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory,
        ILogger<FileUploadedConsumer> logger)
    {
        _connectionFactory = connectionFactory;
        _configuration = configuration;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = _connectionFactory.CreateConsumerConfig(
            "Kafka:Consumers:FileUploaded:GroupId",
            "messaging-file-uploaded");

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        var topic = KafkaTopics.FileUploaded(_configuration);
        consumer.Subscribe(topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(stoppingToken);
                var message = JsonSerializer.Deserialize<FileUploadedNotification>(result.Message.Value);
                if (message is null)
                {
                    continue;
                }

                using var scope = _scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<FileUploadedMessageHandler>();
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
