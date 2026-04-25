using AiTodoApp.Application.DTOs;
using AiTodoApp.Domain.Entities;

namespace AiTodoApp.Application.Mappings;

public static class AppointmentMappings
{
    public static AppointmentDto ToDto(this Appointment appointment) =>
        new(
            appointment.Id,
            appointment.UserId,
            appointment.Title,
            appointment.Description,
            appointment.StartTime,
            appointment.EndTime,
            appointment.Profile,
            appointment.Status,
            appointment.Source,
            appointment.CreatedAt);
}
