using AiTodoApp.Application.DTOs;
using AiTodoApp.Domain.Enums;
using MediatR;

namespace AiTodoApp.Application.Commands.CreateAppointment;

public record CreateAppointmentCommand(
    Guid UserId,
    string Title,
    string Description,
    DateTime StartTime,
    DateTime EndTime,
    string Profile,
    AppointmentStatus Status,
    AppointmentSource Source
) : IRequest<AppointmentDto>;
