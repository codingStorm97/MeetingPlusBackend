namespace AiTodoApp.Application.DTOs;

public sealed record ConversationEventDto(
    string Title,
    DateTime StartTime,
    DateTime EndTime,
    string? Description);
