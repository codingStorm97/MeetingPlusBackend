using AiTodoApp.Application.DTOs;
using MediatR;

namespace AiTodoApp.Application.Queries.GetAllAppointments;

public record GetAllAppointmentsQuery(Guid UserId) : IRequest<IReadOnlyList<AppointmentDto>>;
