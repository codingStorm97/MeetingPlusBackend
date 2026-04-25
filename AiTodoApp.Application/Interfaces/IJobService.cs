using AiTodoApp.Application.DTOs;

namespace AiTodoApp.Application.Interfaces;

public interface IJobService
{
    Task<JobItemDto> CreatePendingJobAsync(
        Guid userId,
        string fileName,
        string relativePath,
        CancellationToken cancellationToken = default);

    Task UpdateJobStatusAsync(
        Guid jobId,
        string status,
        DateTime timeUtc,
        string? error = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<JobItemDto>> GetJobsByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<int> DeleteByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
