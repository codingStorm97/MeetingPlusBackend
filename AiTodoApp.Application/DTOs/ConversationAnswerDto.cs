namespace AiTodoApp.Application.DTOs;

public sealed record ConversationAnswerDto(
    Guid ConversationId,
    Guid QuestionId,
    string Question,
    string Answer,
    string Intent,
    ConversationEventDto? Event);
