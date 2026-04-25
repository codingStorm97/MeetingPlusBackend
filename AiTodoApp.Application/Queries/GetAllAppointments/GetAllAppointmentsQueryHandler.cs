using AiTodoApp.Application.DTOs;
using AiTodoApp.Application.Interfaces;
using AiTodoApp.Application.Mappings;
using MediatR;

namespace AiTodoApp.Application.Queries.GetAllAppointments;

public class GetAllAppointmentsQueryHandler : IRequestHandler<GetAllAppointmentsQuery, IReadOnlyList<AppointmentDto>>
{
    private readonly IAppointmentRepository _appointmentRepository;

    public GetAllAppointmentsQueryHandler(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    public async Task<IReadOnlyList<AppointmentDto>> Handle(GetAllAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var appointments = await _appointmentRepository.GetAllAsync(request.UserId, cancellationToken);
        return appointments.Select(x => x.ToDto()).ToList();
    }
}
