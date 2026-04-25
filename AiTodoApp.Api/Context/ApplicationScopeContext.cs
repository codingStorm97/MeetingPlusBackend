using AiTodoApp.Application.Interfaces;

namespace AiTodoApp.Api.Context;

public class ApplicationScopeContext : IApplicationScopeContext
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserFolderPath { get; set; } = string.Empty;
}
