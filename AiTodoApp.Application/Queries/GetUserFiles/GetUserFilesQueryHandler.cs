using AiTodoApp.Application.DTOs;
using AiTodoApp.Application.Interfaces;
using MediatR;

namespace AiTodoApp.Application.Queries.GetUserFiles;

public class GetUserFilesQueryHandler : IRequestHandler<GetUserFilesQuery, UserFilesResultDto>
{
    private readonly IApplicationScopeContext _applicationScopeContext;
    private readonly IJobService _jobService;

    public GetUserFilesQueryHandler(
        IApplicationScopeContext applicationScopeContext,
        IJobService jobService)
    {
        _applicationScopeContext = applicationScopeContext;
        _jobService = jobService;
    }

    public async Task<UserFilesResultDto> Handle(GetUserFilesQuery request, CancellationToken cancellationToken)
    {
        var jobs = await _jobService.GetJobsByUserAsync(_applicationScopeContext.UserId, cancellationToken);
        var mapped = jobs
            .Select(job => new UserFileResultItemDto(
                job.UploadFileId,
                job.UploadFileName,
                job.Status,
                job.RelativePath,
                $"/api/files/download/{Uri.EscapeDataString(job.UploadFileName)}"))
            .ToList();

        return new UserFilesResultDto(
            _applicationScopeContext.UserName,
            _applicationScopeContext.UserFolderPath,
            mapped);
    }
}
