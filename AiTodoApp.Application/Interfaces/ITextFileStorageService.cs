using AiTodoApp.Application.DTOs;

namespace AiTodoApp.Application.Interfaces;

public interface ITextFileStorageService
{
    Task<string> SaveAsync(
        string userName,
        string fileName,
        byte[] content,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserFileDto>> GetFilesAsync(
        string userName,
        CancellationToken cancellationToken = default);

    Task<byte[]?> ReadFileAsync(
        string userName,
        string fileName,
        CancellationToken cancellationToken = default);

    Task<int> DeleteAllFilesAsync(
        string userName,
        CancellationToken cancellationToken = default);
}
