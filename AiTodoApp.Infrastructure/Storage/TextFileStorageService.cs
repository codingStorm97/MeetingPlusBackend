using AiTodoApp.Application.DTOs;
using AiTodoApp.Application.Interfaces;
using Microsoft.Extensions.Hosting;

namespace AiTodoApp.Infrastructure.Storage;

public class TextFileStorageService : ITextFileStorageService
{
    private readonly IHostEnvironment _environment;

    public TextFileStorageService(IHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> SaveAsync(
        string userName,
        string fileName,
        byte[] content,
        CancellationToken cancellationToken = default)
    {
        var userDataDirectory = Path.Combine(_environment.ContentRootPath, "Data", userName);
        Directory.CreateDirectory(userDataDirectory);

        var safeFileName = Path.GetFileName(fileName);
        var targetFilePath = Path.Combine(userDataDirectory, safeFileName);

        await File.WriteAllBytesAsync(targetFilePath, content, cancellationToken);
        return $"Data/{userName}/{safeFileName}";
    }

    public Task<IReadOnlyList<UserFileDto>> GetFilesAsync(
        string userName,
        CancellationToken cancellationToken = default)
    {
        var userDataDirectory = Path.Combine(_environment.ContentRootPath, "Data", userName);
        if (!Directory.Exists(userDataDirectory))
        {
            return Task.FromResult<IReadOnlyList<UserFileDto>>(Array.Empty<UserFileDto>());
        }

        var files = Directory.GetFiles(userDataDirectory, "*.txt", SearchOption.TopDirectoryOnly)
            .Select(path =>
            {
                var fileName = Path.GetFileName(path);
                return new UserFileDto(fileName, $"Data/{userName}/{fileName}");
            })
            .ToList();

        return Task.FromResult<IReadOnlyList<UserFileDto>>(files);
    }

    public async Task<byte[]?> ReadFileAsync(
        string userName,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var safeFileName = Path.GetFileName(fileName);
        var userDataDirectory = Path.Combine(_environment.ContentRootPath, "Data", userName);
        var targetPath = Path.Combine(userDataDirectory, safeFileName);

        if (!File.Exists(targetPath))
        {
            return null;
        }

        return await File.ReadAllBytesAsync(targetPath, cancellationToken);
    }

    public Task<int> DeleteAllFilesAsync(
        string userName,
        CancellationToken cancellationToken = default)
    {
        var userDataDirectory = Path.Combine(_environment.ContentRootPath, "Data", userName);
        if (!Directory.Exists(userDataDirectory))
        {
            return Task.FromResult(0);
        }

        var files = Directory.GetFiles(userDataDirectory, "*", SearchOption.TopDirectoryOnly);
        foreach (var filePath in files)
        {
            File.Delete(filePath);
        }

        return Task.FromResult(files.Length);
    }
}
