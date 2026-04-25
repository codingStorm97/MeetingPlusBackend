using AiTodoApp.Application.DTOs;
using MediatR;

namespace AiTodoApp.Application.Commands.RegisterUser;

public record RegisterUserCommand(string Email, string Password) : IRequest<AuthResponseDto?>;
