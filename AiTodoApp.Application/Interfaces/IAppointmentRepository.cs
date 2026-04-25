using AiTodoApp.Domain.Entities;

namespace AiTodoApp.Application.Interfaces;

public interface IAppointmentRepository
{
    Task<IReadOnlyList<Appointment>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Appointment?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default);
}
