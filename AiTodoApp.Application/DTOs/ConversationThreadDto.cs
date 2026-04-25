namespace AiTodoApp.Application.DTOs;

public sealed record ConversationThreadDto(
    Guid ConversationId,
    string Title,
    DateTime CreatedAt,
    IReadOnlyList<ConversationQuestionDto> Questions);
