namespace AiTodoApp.Domain.Entities;

public class ConversationQuestion
{
    public Guid QuestionId { get; set; } = Guid.NewGuid();
    public Guid ConversationId { get; set; }
    public Guid UserId { get; set; }
    public string Question { get; set; } = string.Empty;
    public string? Answer { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AnsweredTime { get; set; }

    public Conversation Conversation { get; set; } = null!;
    public User User { get; set; } = null!;
}
