using Confluent.Kafka;
using System.Text.Json;
using AiTodoApp.Messaging.Contracts;
using AiTodoApp.VectorDB.Services;

namespace AiTodoApp.VectorDB;

public class Worker : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly EmbeddingJobProcessor _embeddingJobProcessor;
    private readonly ILogger<Worker> _logger;

    public Worker(
        IConfiguration configuration,
        EmbeddingJobProcessor embeddingJobProcessor,
        ILogger<Worker> logger)
    {
        _configuration = configuration;
        _embeddingJobProcessor = embeddingJobProcessor;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var bootstrapServers = _configuration["Kafka:BootstrapServers"] ?? "localhost:9050";
        var embeddingTopic = _configuration["Kafka:Topics:EmbeddingJobRequested"] ?? "embedding-job-requested";
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = "vectordb-worker",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };

        using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
        consumer.Subscribe(embeddingTopic);

        _logger.LogInformation("Connected to Kafka at: {BootstrapServers}", bootstrapServers);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumed = consumer.Consume(stoppingToken);
                var requested = JsonSerializer.Deserialize<EmbeddingJobRequestedMessage>(consumed.Message.Value);
                if (requested is null)
                {
                    continue;
                }

                await _embeddingJobProcessor.ProcessAsync(requested, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to consume embedding job from Kafka.");
            }
        }
    }
}
