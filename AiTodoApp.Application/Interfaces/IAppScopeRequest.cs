namespace AiTodoApp.Application.Interfaces;

public interface IAppScopeRequest
{
    Guid UserId { get; set; }
    string UserName { get; set; }
    string UserFolderPath { get; set; }
}
