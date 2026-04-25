using AiTodoApp.Api.Filters;
using AiTodoApp.Application.Commands.SemanticSearch;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiTodoApp.Api.Controllers;

[ApiController]
[Authorize]
[ServiceFilter(typeof(ApplicationScopeFilter))]
[Route("api/search")]
public class SearchController(
    IMediator mediator) : ControllerBase
{
    [HttpPost("semantic")]
    public async Task<IActionResult> SemanticSearch(
        [FromBody] SemanticSearchRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
        {
            return BadRequest(new { message = "text is required." });
        }

        var result = await mediator.Send(
            new SemanticSearchCommand(request.Text, request.TopK),
            cancellationToken);
        return Ok(result);
    }
}

public sealed record SemanticSearchRequest(string Text, int? TopK);
