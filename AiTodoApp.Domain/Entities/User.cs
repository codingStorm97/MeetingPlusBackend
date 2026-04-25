namespace AiTodoApp.Domain.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string TimeZone { get; set; } = "UTC";
    public string? GoogleAccountEmail { get; set; }
    public string? GoogleCalendarId { get; set; }
    public string? GoogleRefreshToken { get; set; }
    public DateTime? GoogleTokenExpiresAt { get; set; }
    public bool IsGoogleConnected { get; set; }
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
    public ICollection<ConversationQuestion> ConversationQuestions { get; set; } = new List<ConversationQuestion>();
}
