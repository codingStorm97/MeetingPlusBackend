using AiTodoApp.Application.DTOs;
using AiTodoApp.Application.Interfaces;
using AiTodoApp.Messaging.Contracts;
using MediatR;

namespace AiTodoApp.Application.Commands.UploadTextFile;

public class UploadTextFileCommandHandler : IRequestHandler<UploadTextFileCommand, FileUploadResultDto>
{
    private readonly ITextFileStorageService _textFileStorageService;
    private readonly IFileHistoryService _fileHistoryService;
    private readonly IJobService _jobService;
    private readonly IMessagingPublisher _messagingPublisher;

    public UploadTextFileCommandHandler(
        ITextFileStorageService textFileStorageService,
        IFileHistoryService fileHistoryService,
        IJobService jobService,
        IMessagingPublisher messagingPublisher)
    {
        _textFileStorageService = textFileStorageService;
        _fileHistoryService = fileHistoryService;
        _jobService = jobService;
        _messagingPublisher = messagingPublisher;
    }

    public async Task<FileUploadResultDto> Handle(UploadTextFileCommand request, CancellationToken cancellationToken)
    {
        var relativePath = await _textFileStorageService.SaveAsync(
            request.UserName,
            request.FileName,
            request.Content,
            cancellationToken);

        await _fileHistoryService.AddFileHistoryAsync(
            request.UserId,
            request.UserName,
            request.FileName,
            relativePath,
            DateTime.UtcNow,
            cancellationToken);

        var job = await _jobService.CreatePendingJobAsync(
            request.UserId,
            request.FileName,
            relativePath,
            cancellationToken);

        await _jobService.UpdateJobStatusAsync(
            job.JobId,
            "Uploaded",
            DateTime.UtcNow,
            null,
            cancellationToken);

        var response = new FileUploadResultDto(
            job.JobId,
            request.UserId,
            request.UserName,
            request.FileName,
            "File uploaded successfully.",
            relativePath,
            DateTime.UtcNow);

        await _messagingPublisher.PublishFileUploadedAsync(
            new FileUploadedNotification(
                response.JobId,
                response.UserId,
                response.UserName,
                response.FileName,
                response.Path,
                response.TimeUtc),
            cancellationToken);

        return response;
    }
}
