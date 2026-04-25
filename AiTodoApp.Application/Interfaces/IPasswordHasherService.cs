using AiTodoApp.Domain.Entities;

namespace AiTodoApp.Application.Interfaces;

public interface IPasswordHasherService
{
    string HashPassword(User user, string password);
    bool VerifyPassword(User user, string hashedPassword, string providedPassword);
}
