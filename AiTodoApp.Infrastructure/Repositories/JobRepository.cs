using AiTodoApp.Application.DTOs;
using AiTodoApp.Application.Interfaces;
using AiTodoApp.Infrastructure.Persistence;
using AiTodoApp.Infrastructure.Queries;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AiTodoApp.Infrastructure.Repositories;

public class JobRepository : IJobRepository
{
    private readonly AppDbContext _dbContext;

    public JobRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task InsertPendingAsync(JobItemDto item, CancellationToken cancellationToken = default)
    {
        await EnsureTableAsync(cancellationToken);

        await _dbContext.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO [dbo].[job] ([id], [userId], [jobStatus], [time], [uploadFileId], [uploadFileName], [relativePath])
            VALUES (@id, @userId, @jobStatus, @time, @uploadFileId, @uploadFileName, @relativePath)
            """,
            new SqlParameter("@id", item.JobId),
            new SqlParameter("@userId", item.UserId),
            new SqlParameter("@jobStatus", item.Status),
            new SqlParameter("@time", item.TimeUtc),
            new SqlParameter("@uploadFileId", item.UploadFileId),
            new SqlParameter("@uploadFileName", item.UploadFileName),
            new SqlParameter("@relativePath", item.RelativePath));
    }

    public async Task UpdateStatusAsync(
        Guid jobId,
        string status,
        DateTime timeUtc,
        string? error = null,
        CancellationToken cancellationToken = default)
    {
        await EnsureTableAsync(cancellationToken);

        await _dbContext.Database.ExecuteSqlRawAsync(
            """
            UPDATE [dbo].[job]
            SET [jobStatus] = @jobStatus,
                [time] = @time,
                [error] = @error,
                [attempts] = CASE WHEN @jobStatus = 'Failed' THEN [attempts] + 1 ELSE [attempts] END
            WHERE [id] = @id
            """,
            new SqlParameter("@jobStatus", status),
            new SqlParameter("@time", timeUtc),
            new SqlParameter("@error", (object?)error ?? DBNull.Value),
            new SqlParameter("@id", jobId));
    }

    public async Task<IReadOnlyList<JobItemDto>> GetByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        await EnsureTableAsync(cancellationToken);

        var rows = await _dbContext.Database.SqlQueryRaw<JobSqlRow>(
            """
            SELECT [id], [userId], [jobStatus], [time], [uploadFileId], [uploadFileName], [relativePath]
            FROM [dbo].[job]
            WHERE [userId] = @userId
            ORDER BY [time] DESC
            """,
            new SqlParameter("@userId", userId))
            .ToListAsync(cancellationToken);

        return rows.Select(r => new JobItemDto(
            r.Id,
            r.UserId,
            r.JobStatus,
            r.Time,
            r.UploadFileId,
            r.UploadFileName,
            r.RelativePath)).ToList();
    }

    public async Task<int> DeleteByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        await EnsureTableAsync(cancellationToken);

        return await _dbContext.Database.ExecuteSqlRawAsync(
            """
            DELETE FROM [dbo].[job]
            WHERE [userId] = @userId
            """,
            new SqlParameter("@userId", userId));
    }

    private async Task EnsureTableAsync(CancellationToken cancellationToken)
    {
        await _dbContext.Database.ExecuteSqlRawAsync(JobTableCreateQuery.Sql, cancellationToken);
    }

    private sealed class JobSqlRow
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string JobStatus { get; set; } = string.Empty;
        public DateTime Time { get; set; }
        public Guid UploadFileId { get; set; }
        public string UploadFileName { get; set; } = string.Empty;
        public string RelativePath { get; set; } = string.Empty;
    }
}
