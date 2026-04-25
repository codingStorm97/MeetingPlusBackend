using AiTodoApp.Application.DTOs;
using AiTodoApp.Application.Interfaces;
using AiTodoApp.Domain.Entities;
using MediatR;

namespace AiTodoApp.Application.Commands.RegisterUser;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, AuthResponseDto?>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasherService _passwordHasherService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasherService passwordHasherService,
        IJwtTokenService jwtTokenService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasherService = passwordHasherService;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponseDto?> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUser is not null)
        {
            return null;
        }

        var user = new User
        {
            Email = request.Email.Trim().ToLowerInvariant(),
            FullName = request.Email.Trim(),
            TimeZone = "UTC",
            IsGoogleConnected = false
        };

        user.Password = _passwordHasherService.HashPassword(user, request.Password);

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var token = _jwtTokenService.GenerateToken(user);
        return new AuthResponseDto(user.Id, user.Email, token);
    }
}
