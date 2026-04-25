using AiTodoApp.Application.DTOs;
using AiTodoApp.Application.Interfaces;

namespace AiTodoApp.Infrastructure.Persistence;

public class JobService : IJobService
{
    private readonly IJobRepository _jobRepository;

    public JobService(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public async Task<JobItemDto> CreatePendingJobAsync(
        Guid userId,
        string fileName,
        string relativePath,
        CancellationToken cancellationToken = default)
    {
        var jobId = Guid.NewGuid();
        var uploadFileId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var item = new JobItemDto(jobId, userId, "Pending", now, uploadFileId, fileName, relativePath);

        await _jobRepository.InsertPendingAsync(item, cancellationToken);
        return item;
    }

    public async Task UpdateJobStatusAsync(
        Guid jobId,
        string status,
        DateTime timeUtc,
        string? error = null,
        CancellationToken cancellationToken = default)
    {
        await _jobRepository.UpdateStatusAsync(jobId, status, timeUtc, error, cancellationToken);
    }

    public async Task<IReadOnlyList<JobItemDto>> GetJobsByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _jobRepository.GetByUserAsync(userId, cancellationToken);
    }

    public async Task<int> DeleteByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _jobRepository.DeleteByUserAsync(userId, cancellationToken);
    }
}
