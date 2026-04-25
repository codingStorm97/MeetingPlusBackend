using System.Text.Json;
using AiTodoApp.Messaging.Contracts;
using Confluent.Kafka;

namespace AiTodoApp.Api.Services;

public class KafkaNotificationConsumerService : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly NotificationStreamService _notificationStreamService;
    private readonly ILogger<KafkaNotificationConsumerService> _logger;

    public KafkaNotificationConsumerService(
        IConfiguration configuration,
        NotificationStreamService notificationStreamService,
        ILogger<KafkaNotificationConsumerService> logger)
    {
        _configuration = configuration;
        _notificationStreamService = notificationStreamService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var bootstrapServers = _configuration["Kafka:BootstrapServers"] ?? "localhost:9050";
        var notificationsTopic = _configuration["Kafka:Topics:UserNotifications"] ?? "user-notifications";
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = "api-notifications",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };

        using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
        consumer.Subscribe(notificationsTopic);
        _logger.LogInformation("Kafka notification consumer started at {BootstrapServers}", bootstrapServers);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumed = consumer.Consume(stoppingToken);
                HandleMessage(consumed.Message.Value);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kafka notification consume failed.");
            }
        }
    }

    private void HandleMessage(string payload)
    {
        var notification = JsonSerializer.Deserialize<UserNotificationMessage>(payload);
        if (notification is null)
        {
            return;
        }

        if (notification.EventType == "fileUploaded")
        {
            var uploaded = JsonSerializer.Deserialize<FileUploadedNotification>(notification.PayloadJson);
            if (uploaded is not null)
            {
                _notificationStreamService.PublishFileUploaded(uploaded);
            }
            return;
        }

        if (notification.EventType != "jobStatusChanged")
        {
            return;
        }

        var status = JsonSerializer.Deserialize<JobStatusChangedMessage>(notification.PayloadJson);
        if (status is null)
        {
            return;
        }

        _notificationStreamService.PublishJobStatusChanged(status);
    }
}
