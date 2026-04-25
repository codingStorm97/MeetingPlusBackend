using AiTodoApp.HuggingFaceMcp.Application.Handlers;
using AiTodoApp.HuggingFaceMcp.Application.Interfaces;
using AiTodoApp.HuggingFaceMcp.Application.Services;
using AiTodoApp.HuggingFaceMcp.Infrastructure;
using StackExchange.Redis;

DotEnvLoader.LoadSharedEnvironmentVariables();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient<IHuggingFaceContext, HuggingFaceContext>();
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
{
    var redisConnection = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
    return ConnectionMultiplexer.Connect(redisConnection);
});
builder.Services.AddScoped<IConversationHistoryStore, RedisConversationHistoryStore>();
builder.Services.AddScoped<IConversationAnswerService, ConversationAnswerService>();
builder.Services.AddScoped<IToolRegistry, ToolRegistry>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<GetConversationResultQueryHandler>());

var app = builder.Build();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
