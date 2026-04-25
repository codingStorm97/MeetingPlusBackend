using AiTodoApp.Domain.Entities;

namespace AiTodoApp.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}
