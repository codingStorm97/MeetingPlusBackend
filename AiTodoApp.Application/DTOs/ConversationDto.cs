namespace AiTodoApp.Application.DTOs;

public sealed record ConversationDto(
    Guid ConversationId,
    string ConversationName,
    DateTime CreatedAt);
