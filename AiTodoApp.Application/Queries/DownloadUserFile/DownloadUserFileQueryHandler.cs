using AiTodoApp.Application.DTOs;
using AiTodoApp.Application.Interfaces;
using MediatR;

namespace AiTodoApp.Application.Queries.DownloadUserFile;

public class DownloadUserFileQueryHandler : IRequestHandler<DownloadUserFileQuery, FileDownloadResultDto?>
{
    private readonly IApplicationScopeContext _applicationScopeContext;
    private readonly ITextFileStorageService _textFileStorageService;

    public DownloadUserFileQueryHandler(
        IApplicationScopeContext applicationScopeContext,
        ITextFileStorageService textFileStorageService)
    {
        _applicationScopeContext = applicationScopeContext;
        _textFileStorageService = textFileStorageService;
    }

    public async Task<FileDownloadResultDto?> Handle(DownloadUserFileQuery request, CancellationToken cancellationToken)
    {
        if (!string.Equals(Path.GetExtension(request.FileName), ".txt", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var safeFileName = Path.GetFileName(request.FileName);
        var content = await _textFileStorageService.ReadFileAsync(
            _applicationScopeContext.UserName,
            safeFileName,
            cancellationToken);

        return content is null ? null : new FileDownloadResultDto(safeFileName, content);
    }
}
