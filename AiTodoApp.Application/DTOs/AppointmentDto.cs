using AiTodoApp.Domain.Enums;

namespace AiTodoApp.Application.DTOs;

public record AppointmentDto(
    Guid Id,
    Guid UserId,
    string Title,
    string Description,
    DateTime StartTime,
    DateTime EndTime,
    string Profile,
    AppointmentStatus Status,
    AppointmentSource Source,
    DateTime CreatedAt
);
