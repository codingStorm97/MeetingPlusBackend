namespace AiTodoApp.Application.DTOs;

public sealed record ConversationQuestionDto(
    Guid QuestionId,
    string Question,
    string? Answer,
    DateTime CreatedAt,
    DateTime? AnsweredTime);
