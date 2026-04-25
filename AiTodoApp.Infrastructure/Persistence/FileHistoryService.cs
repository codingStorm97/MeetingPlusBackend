using AiTodoApp.Application.Interfaces;
using AiTodoApp.Infrastructure.Queries;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AiTodoApp.Infrastructure.Persistence;

public class FileHistoryService : IFileHistoryService
{
    private readonly AppDbContext _dbContext;

    public FileHistoryService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddFileHistoryAsync(
        Guid userId,
        string userName,
        string fileName,
        string relativePath,
        DateTime uploadedAtUtc,
        CancellationToken cancellationToken = default)
    {
        await EnsureFileHistoryTableExistsAsync(cancellationToken);

        await _dbContext.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO [dbo].[FileHistory] ([Id], [UserId], [UserName], [FileName], [RelativePath], [UploadedAtUtc])
            VALUES (@Id, @UserId, @UserName, @FileName, @RelativePath, @UploadedAtUtc)
            """,
            new SqlParameter("@Id", Guid.NewGuid()),
            new SqlParameter("@UserId", userId),
            new SqlParameter("@UserName", userName),
            new SqlParameter("@FileName", fileName),
            new SqlParameter("@RelativePath", relativePath),
            new SqlParameter("@UploadedAtUtc", uploadedAtUtc));
    }

    public async Task<int> DeleteByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        await EnsureFileHistoryTableExistsAsync(cancellationToken);

        return await _dbContext.Database.ExecuteSqlRawAsync(
            """
            DELETE FROM [dbo].[FileHistory]
            WHERE [UserId] = @UserId
            """,
            new SqlParameter("@UserId", userId));
    }

    private async Task EnsureFileHistoryTableExistsAsync(CancellationToken cancellationToken)
    {
        await _dbContext.Database.ExecuteSqlRawAsync(FileHistoryTableCreateQuery.Sql, cancellationToken);
    }
}
