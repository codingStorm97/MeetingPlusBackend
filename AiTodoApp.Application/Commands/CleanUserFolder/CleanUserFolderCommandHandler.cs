using AiTodoApp.Application.DTOs;
using AiTodoApp.Application.Interfaces;
using MediatR;

namespace AiTodoApp.Application.Commands.CleanUserFolder;

public class CleanUserFolderCommandHandler : IRequestHandler<CleanUserFolderCommand, CleanUserFolderResultDto>
{
    private readonly ITextFileStorageService _textFileStorageService;
    private readonly IJobService _jobService;
    private readonly IFileHistoryService _fileHistoryService;

    public CleanUserFolderCommandHandler(
        ITextFileStorageService textFileStorageService,
        IJobService jobService,
        IFileHistoryService fileHistoryService)
    {
        _textFileStorageService = textFileStorageService;
        _jobService = jobService;
        _fileHistoryService = fileHistoryService;
    }

    public async Task<CleanUserFolderResultDto> Handle(CleanUserFolderCommand request, CancellationToken cancellationToken)
    {
        var deletedFileCount = await _textFileStorageService.DeleteAllFilesAsync(request.UserName, cancellationToken);
        var deletedJobRecordCount = await _jobService.DeleteByUserAsync(request.UserId, cancellationToken);
        var deletedFileHistoryRecordCount = await _fileHistoryService.DeleteByUserAsync(request.UserId, cancellationToken);

        return new CleanUserFolderResultDto(
            request.UserId,
            request.UserName,
            deletedFileCount,
            deletedJobRecordCount,
            deletedFileHistoryRecordCount);
    }
}
