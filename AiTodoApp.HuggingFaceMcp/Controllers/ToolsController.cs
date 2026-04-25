using AiTodoApp.HuggingFaceMcp.Application.Interfaces;
using AiTodoApp.HuggingFaceMcp.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace AiTodoApp.HuggingFaceMcp.Controllers;

[ApiController]
[Route("api/tools")]
public class ToolsController(IToolRegistry toolRegistry) : ControllerBase
{
    [HttpGet]
    public ActionResult<object> GetTools()
    {
        var tools = toolRegistry.GetTools();
        return Ok(new { tools });
    }

    [HttpPost("execute")]
    public async Task<ActionResult<ToolExecuteResponse>> Execute(
        [FromBody] ToolExecuteRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Tool))
        {
            return BadRequest(ToolExecuteResponse.Failure("validation_error", "Field 'tool' is required."));
        }

        try
        {
            var result = await toolRegistry.ExecuteAsync(
                request.Tool,
                request.Arguments ?? []);

            return Ok(ToolExecuteResponse.Success(result));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ToolExecuteResponse.Failure("validation_error", ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ToolExecuteResponse.Failure("tool_not_found", ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ToolExecuteResponse.Failure("execution_error", ex.Message));
        }
    }
}
