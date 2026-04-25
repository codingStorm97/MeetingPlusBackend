using AiTodoApp.Application.Interfaces;
using AiTodoApp.Infrastructure.Auth;
using AiTodoApp.Infrastructure.Messaging;
using AiTodoApp.Infrastructure.Persistence;
using AiTodoApp.Infrastructure.Repositories;
using AiTodoApp.Infrastructure.Storage;
using AiTodoApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AiTodoApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var baseConnectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

        var databaseName = configuration["DatabaseSettings:Name"]
            ?? throw new InvalidOperationException("DatabaseSettings:Name is missing.");

        var databasePassword = configuration["DatabaseSettings:Password"]
            ?? throw new InvalidOperationException("DatabaseSettings:Password is missing.");

        var connectionStringBuilder = new SqlConnectionStringBuilder(baseConnectionString)
        {
            InitialCatalog = databaseName,
            Password = databasePassword
        };

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionStringBuilder.ConnectionString));

        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<DatabaseInitializer>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasherService, PasswordHasherService>();
        services.AddScoped<ITextFileStorageService, TextFileStorageService>();
        services.AddScoped<IFileHistoryService, FileHistoryService>();
        services.AddScoped<IJobRepository, JobRepository>();
        services.AddScoped<IJobService, JobService>();
        services.AddScoped<IMessagingPublisher, MessagingPublisher>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AppDbContext>());

        return services;
    }
}
