using AiTodoApp.Application.DTOs;
using AiTodoApp.Application.Interfaces;
using AiTodoApp.Application.Mappings;
using AiTodoApp.Domain.Entities;
using MediatR;

namespace AiTodoApp.Application.Commands.CreateAppointment;

public class CreateAppointmentCommandHandler : IRequestHandler<CreateAppointmentCommand, AppointmentDto>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAppointmentCommandHandler(IAppointmentRepository appointmentRepository, IUnitOfWork unitOfWork)
    {
        _appointmentRepository = appointmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AppointmentDto> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = new Appointment
        {
            UserId = request.UserId,
            Title = request.Title,
            Description = request.Description,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Profile = request.Profile,
            Status = request.Status,
            Source = request.Source,
            CreatedAt = DateTime.UtcNow
        };

        await _appointmentRepository.AddAsync(appointment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return appointment.ToDto();
    }
}
