using AiTodoApp.Application.Interfaces;
using AiTodoApp.Domain.Entities;
using AiTodoApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AiTodoApp.Infrastructure.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly AppDbContext _dbContext;

    public AppointmentRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Appointment>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Appointments
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<Appointment?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Appointments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Id == id, cancellationToken);
    }

    public async Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        await _dbContext.Appointments.AddAsync(appointment, cancellationToken);
    }
}
