using AiTodoApp.Application.Commands.UploadTextFile;
using AiTodoApp.Application.Commands.CleanUserFolder;
using AiTodoApp.Application.Queries.DownloadUserFile;
using AiTodoApp.Application.Queries.GetUserFiles;
using AiTodoApp.Api.Contracts;
using AiTodoApp.Api.Filters;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiTodoApp.Api.Controllers;

[ApiController]
[Authorize]
[ServiceFilter(typeof(ApplicationScopeFilter))]
[Route("api/files")]
public class FilesController : ControllerBase
{
    private readonly IMediator _mediator;

    public FilesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Consumes("multipart/form-data")]
    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] UploadTextFileRequest request, CancellationToken cancellationToken)
    {
        var file = request.File;
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "File is required." });
        }

        if (!string.Equals(Path.GetExtension(file.FileName), ".txt", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Only .txt files are allowed." });
        }

        await using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, cancellationToken);

        var result = await _mediator.Send(
            new UploadTextFileCommand(file.FileName, memoryStream.ToArray()),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetFiles(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUserFilesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("download/{fileName}")]
    public async Task<IActionResult> Download([FromRoute] string fileName, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DownloadUserFileQuery(fileName), cancellationToken);
        return result is null
            ? NotFound(new { message = "File not found." })
            : File(result.Content, "text/plain", result.FileName);
    }

    [HttpDelete("clean-folder")]
    public async Task<IActionResult> CleanFolder(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CleanUserFolderCommand(), cancellationToken);
        return Ok(result);
    }
}
