using AiTodoApp.Application.Interfaces;
using AiTodoApp.Application.DTOs;
using MediatR;

namespace AiTodoApp.Application.Commands.UploadTextFile;

public record UploadTextFileCommand(
    string FileName,
    byte[] Content
) : IRequest<FileUploadResultDto>, IAppScopeRequest
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserFolderPath { get; set; } = string.Empty;
}
