namespace AiTodoApp.Application.DTOs;

public record AuthResponseDto(
    Guid UserId,
    string Email,
    string Token
);
