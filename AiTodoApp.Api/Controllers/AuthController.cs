using AiTodoApp.Api.Contracts;
using AiTodoApp.Application.Commands.AuthenticateUser;
using AiTodoApp.Application.Commands.RegisterUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiTodoApp.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new AuthenticateUserCommand(request.Email, request.Password), cancellationToken);
        return result is null ? Unauthorized(new { message = "Invalid credentials." }) : Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RegisterUserCommand(request.Email, request.Password), cancellationToken);
        return result is null ? Conflict(new { message = "Email already exists." }) : Ok(result);
    }
}
