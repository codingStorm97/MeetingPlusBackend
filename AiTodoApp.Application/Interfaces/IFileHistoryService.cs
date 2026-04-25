namespace AiTodoApp.Application.Interfaces;

public interface IFileHistoryService
{
    Task AddFileHistoryAsync(
        Guid userId,
        string userName,
        string fileName,
        string relativePath,
        DateTime uploadedAtUtc,
        CancellationToken cancellationToken = default);

    Task<int> DeleteByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
