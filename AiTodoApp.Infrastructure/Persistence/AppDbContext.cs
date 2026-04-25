using AiTodoApp.Application.Interfaces;
using AiTodoApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AiTodoApp.Infrastructure.Persistence;

public class AppDbContext : DbContext, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ConversationQuestion> ConversationQuestions => Set<ConversationQuestion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.Email).IsRequired().HasMaxLength(256);
            entity.Property(x => x.FullName).IsRequired().HasMaxLength(150);
            entity.Property(x => x.Password).IsRequired().HasMaxLength(250);
            entity.Property(x => x.TimeZone).IsRequired().HasMaxLength(64);
            entity.Property(x => x.GoogleAccountEmail).HasMaxLength(256);
            entity.Property(x => x.GoogleCalendarId).HasMaxLength(256);
            entity.Property(x => x.GoogleRefreshToken).HasMaxLength(2048);
            entity.Property(x => x.GoogleTokenExpiresAt);
            entity.Property(x => x.IsGoogleConnected).IsRequired();
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.ToTable("Appointments");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.UserId);

            entity.Property(x => x.UserId).IsRequired();
            entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Description).HasMaxLength(2000);
            entity.Property(x => x.Profile).IsRequired().HasMaxLength(150);
            entity.Property(x => x.Status).IsRequired();
            entity.Property(x => x.Source).IsRequired();
            entity.Property(x => x.StartTime).IsRequired();
            entity.Property(x => x.EndTime).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.HasOne(x => x.User)
                .WithMany(x => x.Appointments)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.ToTable("Conversation");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.UserId);
            entity.Property(x => x.UserId).IsRequired();
            entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.HasOne(x => x.User)
                .WithMany(x => x.Conversations)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ConversationQuestion>(entity =>
        {
            entity.ToTable("ConversationQuestion");
            entity.HasKey(x => x.QuestionId);
            entity.HasIndex(x => x.ConversationId);
            entity.HasIndex(x => x.UserId);
            entity.Property(x => x.Question).IsRequired().HasMaxLength(4000);
            entity.Property(x => x.Answer).HasMaxLength(4000);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.AnsweredTime);
            entity.HasOne(x => x.Conversation)
                .WithMany(x => x.Questions)
                .HasForeignKey(x => x.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.User)
                .WithMany(x => x.ConversationQuestions)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }
}
