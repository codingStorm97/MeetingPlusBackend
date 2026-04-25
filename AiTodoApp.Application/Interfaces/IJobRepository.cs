using AiTodoApp.Application.DTOs;

namespace AiTodoApp.Application.Interfaces;

public interface IJobRepository
{
    Task InsertPendingAsync(
        JobItemDto item,
        CancellationToken cancellationToken = default);

    Task UpdateStatusAsync(
        Guid jobId,
        string status,
        DateTime timeUtc,
        string? error = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<JobItemDto>> GetByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<int> DeleteByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
