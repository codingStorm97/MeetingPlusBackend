using AiTodoApp.HuggingFaceMcp.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AiTodoApp.HuggingFaceMcp.Controllers;

[ApiController]
[Route("api/conversation")]
[Route("mcp")]
public class ConversationController(IMediator mediator) : ControllerBase
{
    [HttpPost("query")]
    public async Task<ActionResult<ConversationResultDto>> Query(
        [FromBody] ConversationQueryRequest request,
        CancellationToken cancellationToken)
    {
        var query = new GetConversationResultQuery(
            request.ConversationId,
            request.Question,
            request.Context ?? string.Empty);

        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}

public sealed record ConversationQueryRequest(string ConversationId, string Question, string? Context);
