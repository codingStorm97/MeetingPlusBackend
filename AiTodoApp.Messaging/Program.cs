using AiTodoApp.Messaging.Messaging.Consumers;
using AiTodoApp.Messaging.Messaging.Common;
using AiTodoApp.Messaging.Messaging.Producers;
using AiTodoApp.VectorDB.Services;
using AiTodoApp.Infrastructure;
using Confluent.Kafka;

LoadSharedEnvironmentVariables();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSingleton<KafkaConnectionFactory>();
builder.Services.AddSingleton<IProducer<string, string>>(_ =>
{
    var producerConfig = new ProducerConfig { BootstrapServers = builder.Configuration["Kafka:BootstrapServers"] ?? "localhost:9050" };
    return new ProducerBuilder<string, string>(producerConfig).Build();
});
builder.Services.AddScoped<FileUploadedMessageHandler>();
builder.Services.AddScoped<JobStatusChangedMessageHandler>();
builder.Services.AddSingleton<EmbeddingJobRequestedProducer>();
builder.Services.AddSingleton<AiTodoApp.VectorDB.Services.IJobStatusMessageProducer, JobStatusChangedProducer>();
builder.Services.AddSingleton<UserNotificationProducer>();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<TextChunker>();
builder.Services.AddSingleton<OpenAiEmbeddingService>();
builder.Services.AddSingleton<QdrantService>();
builder.Services.AddSingleton<EmbeddingJobProcessor>();
builder.Services.AddHostedService<FileUploadedConsumer>();
builder.Services.AddHostedService<JobStatusChangedConsumer>();
builder.Services.AddHostedService<EmbeddingJobRequestedConsumer>();

var app = builder.Build();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();

static void LoadSharedEnvironmentVariables()
{
    var envPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", ".env"));
    if (!File.Exists(envPath))
    {
        return;
    }

    foreach (var rawLine in File.ReadAllLines(envPath))
    {
        var line = rawLine.Trim();
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
        {
            continue;
        }

        var separatorIndex = line.IndexOf('=');
        if (separatorIndex <= 0)
        {
            continue;
        }

        var key = line[..separatorIndex].Trim();
        var value = line[(separatorIndex + 1)..].Trim().Trim('"');
        if (string.IsNullOrWhiteSpace(key))
        {
            continue;
        }

        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
        {
            Environment.SetEnvironmentVariable(key, value);
        }
    }
}
