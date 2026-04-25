using AiTodoApp.Application.DTOs;
using MediatR;

namespace AiTodoApp.Application.Queries.GetAppointmentById;

public record GetAppointmentByIdQuery(Guid UserId, Guid Id) : IRequest<AppointmentDto?>;
