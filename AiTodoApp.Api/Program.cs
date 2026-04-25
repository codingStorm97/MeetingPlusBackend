using AiTodoApp.Application;
using AiTodoApp.Application.Interfaces;
using AiTodoApp.Agent.Abstractions;
using AiTodoApp.Agent.Services;
using AiTodoApp.Api.Context;
using AiTodoApp.Api.Filters;
using AiTodoApp.Api.Services;
using AiTodoApp.Messaging.HuggingFace;
using AiTodoApp.Infrastructure;
using AiTodoApp.VectorDB.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

LoadSharedEnvironmentVariables();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "AiTodoApp API",
        Version = "v1"
    });

    var bearerSecurityScheme = new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.ParameterLocation.Header,
        Description = "Paste JWT token only (without 'Bearer ')."
    };

    options.AddSecurityDefinition("Bearer", bearerSecurityScheme);

    var bearerSchemeReference = new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer");
    options.AddSecurityRequirement((_) => new Microsoft.OpenApi.OpenApiSecurityRequirement
    {
        [bearerSchemeReference] = new List<string>()
    });

});
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHttpClient<IConversationService, HuggingFaceConversationService>();
builder.Services.AddHttpClient<OpenAiEmbeddingService>();
builder.Services.AddHttpClient<QdrantService>();
builder.Services.AddScoped<ISemanticSearchService, SemanticSearchService>();
builder.Services.AddScoped<AiTodoApp.Application.Interfaces.IAgentService, AgentService>();
builder.Services.AddScoped<IVectorRepository, NoOpVectorRepository>();
builder.Services.AddScoped<ICalendarService, NoOpCalendarService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var secret = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret is missing.");
        var issuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is missing.");
        var audience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience is missing.");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && (path.StartsWithSegments("/api/notifications/stream") || path.StartsWithSegments("/api/notifications/ws")))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddScoped<IApplicationScopeContext, ApplicationScopeContext>();
builder.Services.AddScoped<ApplicationScopeFilter>();
builder.Services.AddSingleton<NotificationStreamService>();
builder.Services.AddHostedService<KafkaNotificationConsumerService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseWebSockets();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

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

        // Do not override externally-provided environment variables.
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
        {
            Environment.SetEnvironmentVariable(key, value);
        }
    }
}
