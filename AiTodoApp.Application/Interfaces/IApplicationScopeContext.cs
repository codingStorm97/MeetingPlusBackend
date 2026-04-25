namespace AiTodoApp.Application.Interfaces;

public interface IApplicationScopeContext
{
    Guid UserId { get; set; }
    string UserName { get; set; }
    string UserFolderPath { get; set; }
}
