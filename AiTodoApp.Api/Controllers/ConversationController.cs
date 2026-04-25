using AiTodoApp.Api.Filters;
using AiTodoApp.Application.Commands.CreateConversation;
using AiTodoApp.Application.Queries.AskConversation;
using AiTodoApp.Application.Queries.GetConversationById;
using AiTodoApp.Application.Queries.GetUserConversations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiTodoApp.Api.Controllers;

[ApiController]
[Authorize]
[ServiceFilter(typeof(ApplicationScopeFilter))]
[Route("api/conversation")]
public class ConversationController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetConversations(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUserConversationsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{conversationId:guid}")]
    public async Task<IActionResult> GetConversationById(Guid conversationId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetConversationByIdQuery(conversationId), cancellationToken);
        return result is null ? NotFound(new { message = "Conversation not found." }) : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateConversation([FromBody] CreateConversationRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateConversationCommand(request.Title), cancellationToken);
        return Ok(result);
    }

    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] AskConversationRequest request, CancellationToken cancellationToken)
    {
        if (request.ConversationId == Guid.Empty)
        {
            return BadRequest(new { message = "conversationId is required." });
        }

        if (string.IsNullOrWhiteSpace(request.Question))
        {
            return BadRequest(new { message = "question is required." });
        }

        try
        {
            var result = await mediator.Send(
                new AskConversationQuery(
                    request.ConversationId,
                    request.Question,
                    request.Context ?? string.Empty),
                cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("query")]
    public Task<IActionResult> Query([FromBody] AskConversationRequest request, CancellationToken cancellationToken)
        => Chat(request, cancellationToken);
}

public sealed record CreateConversationRequest(string? Title);
public sealed record AskConversationRequest(Guid ConversationId, string Question, string? Context);
