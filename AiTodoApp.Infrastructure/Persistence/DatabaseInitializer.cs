using AiTodoApp.Domain.Entities;
using AiTodoApp.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AiTodoApp.Infrastructure.Persistence;

public class DatabaseInitializer
{
    private static readonly Guid SeedUserOneId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid SeedUserTwoId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;

    public DatabaseInitializer(AppDbContext dbContext, IPasswordHasher<User> passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task MigrateAndSeedAsync(CancellationToken cancellationToken = default)
    {
        await MigrateAsync(cancellationToken);
        await EnsureConversationTablesAsync(cancellationToken);

        if (!await _dbContext.Users.AnyAsync(cancellationToken))
        {
            var users = new List<User>
            {
                new()
                {
                    Id = SeedUserOneId,
                    Email = "user1@meetingsplus.local",
                    FullName = "Demo User One",
                    TimeZone = "Asia/Kolkata",
                    GoogleAccountEmail = "user1@gmail.com",
                    GoogleCalendarId = "primary",
                    GoogleRefreshToken = "seed-refresh-token-user-1",
                    GoogleTokenExpiresAt = new DateTime(2027, 4, 23, 0, 0, 0, DateTimeKind.Utc),
                    IsGoogleConnected = true
                },
                new()
                {
                    Id = SeedUserTwoId,
                    Email = "user2@meetingsplus.local",
                    FullName = "Demo User Two",
                    TimeZone = "UTC",
                    IsGoogleConnected = false
                }
            };

            users[0].Password = _passwordHasher.HashPassword(users[0], "Anubhab*1997");
            users[1].Password = _passwordHasher.HashPassword(users[1], "Meeting@123");

            await _dbContext.Users.AddRangeAsync(users, cancellationToken);
        }

        if (!await _dbContext.Appointments.AnyAsync(cancellationToken))
        {
            var appointments = new List<Appointment>
            {
                new()
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    UserId = SeedUserOneId,
                    Title = "Weekly Standup",
                    Description = "Engineering sync for sprint updates.",
                    StartTime = new DateTime(2026, 4, 25, 10, 0, 0, DateTimeKind.Utc),
                    EndTime = new DateTime(2026, 4, 25, 10, 30, 0, 0, DateTimeKind.Utc),
                    Profile = "Engineering",
                    Status = AppointmentStatus.Confirmed,
                    Source = AppointmentSource.Manual,
                    CreatedAt = new DateTime(2026, 4, 23, 0, 0, 0, DateTimeKind.Utc)
                },
                new()
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    UserId = SeedUserTwoId,
                    Title = "Client Requirement Review",
                    Description = "Discuss open action items from client notes.",
                    StartTime = new DateTime(2026, 4, 26, 14, 0, 0, DateTimeKind.Utc),
                    EndTime = new DateTime(2026, 4, 26, 15, 0, 0, DateTimeKind.Utc),
                    Profile = "Product",
                    Status = AppointmentStatus.Pending,
                    Source = AppointmentSource.MeetingNotes,
                    CreatedAt = new DateTime(2026, 4, 23, 0, 0, 0, DateTimeKind.Utc)
                }
            };

            await _dbContext.Appointments.AddRangeAsync(appointments, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.Database.MigrateAsync(cancellationToken);
    }

    private async Task EnsureConversationTablesAsync(CancellationToken cancellationToken)
    {
        const string createConversationSql = """
            IF OBJECT_ID(N'[dbo].[Conversation]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[Conversation](
                    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    [UserId] UNIQUEIDENTIFIER NOT NULL,
                    [Title] NVARCHAR(200) NOT NULL,
                    [CreatedAt] DATETIME2 NOT NULL
                );

                CREATE INDEX [IX_Conversation_UserId] ON [dbo].[Conversation]([UserId]);

                ALTER TABLE [dbo].[Conversation]
                ADD CONSTRAINT [FK_Conversation_Users_UserId]
                FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE;
            END
            """;

        const string createConversationQuestionSql = """
            IF OBJECT_ID(N'[dbo].[ConversationQuestion]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[ConversationQuestion](
                    [QuestionId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    [ConversationId] UNIQUEIDENTIFIER NOT NULL,
                    [UserId] UNIQUEIDENTIFIER NOT NULL,
                    [Question] NVARCHAR(4000) NOT NULL,
                    [Answer] NVARCHAR(4000) NULL,
                    [CreatedAt] DATETIME2 NOT NULL,
                    [AnsweredTime] DATETIME2 NULL
                );

                CREATE INDEX [IX_ConversationQuestion_ConversationId] ON [dbo].[ConversationQuestion]([ConversationId]);
                CREATE INDEX [IX_ConversationQuestion_UserId] ON [dbo].[ConversationQuestion]([UserId]);

                ALTER TABLE [dbo].[ConversationQuestion]
                ADD CONSTRAINT [FK_ConversationQuestion_Conversation_ConversationId]
                FOREIGN KEY ([ConversationId]) REFERENCES [dbo].[Conversation]([Id]) ON DELETE CASCADE;

                ALTER TABLE [dbo].[ConversationQuestion]
                ADD CONSTRAINT [FK_ConversationQuestion_Users_UserId]
                FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]);
            END
            """;

        await _dbContext.Database.ExecuteSqlRawAsync(createConversationSql, cancellationToken);
        await _dbContext.Database.ExecuteSqlRawAsync(createConversationQuestionSql, cancellationToken);
    }
}
