using AiTodoApp.Application.Interfaces;
using AiTodoApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace AiTodoApp.Infrastructure.Auth;

public class PasswordHasherService : IPasswordHasherService
{
    private readonly IPasswordHasher<User> _passwordHasher;

    public PasswordHasherService(IPasswordHasher<User> passwordHasher)
    {
        _passwordHasher = passwordHasher;
    }

    public string HashPassword(User user, string password)
    {
        return _passwordHasher.HashPassword(user, password);
    }

    public bool VerifyPassword(User user, string hashedPassword, string providedPassword)
    {
        var result = _passwordHasher.VerifyHashedPassword(user, hashedPassword, providedPassword);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
