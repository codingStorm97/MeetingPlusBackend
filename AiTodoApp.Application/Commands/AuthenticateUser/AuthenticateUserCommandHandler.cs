using AiTodoApp.Application.DTOs;
using AiTodoApp.Application.Interfaces;
using MediatR;

namespace AiTodoApp.Application.Commands.AuthenticateUser;

public class AuthenticateUserCommandHandler : IRequestHandler<AuthenticateUserCommand, AuthResponseDto?>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasherService _passwordHasherService;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthenticateUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasherService passwordHasherService,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _passwordHasherService = passwordHasherService;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponseDto?> Handle(AuthenticateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null)
        {
            return null;
        }

        if (!_passwordHasherService.VerifyPassword(user, user.Password, request.Password))
        {
            return null;
        }

        var token = _jwtTokenService.GenerateToken(user);
        return new AuthResponseDto(user.Id, user.Email, token);
    }
}
