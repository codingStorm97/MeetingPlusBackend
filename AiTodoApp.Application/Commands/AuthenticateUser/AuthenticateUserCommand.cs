using AiTodoApp.Application.DTOs;
using MediatR;

namespace AiTodoApp.Application.Commands.AuthenticateUser;

public record AuthenticateUserCommand(string Email, string Password) : IRequest<AuthResponseDto?>;
