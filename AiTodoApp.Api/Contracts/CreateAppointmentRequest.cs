using AiTodoApp.Domain.Enums;
using System.Text.Json.Serialization;

namespace AiTodoApp.Api.Contracts;

public class CreateAppointmentRequest
{
    [JsonIgnore]
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Profile { get; set; } = string.Empty;
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    public AppointmentSource Source { get; set; } = AppointmentSource.Manual;
}
