using AiTodoApp.HuggingFaceMcp.Application.Models;

namespace AiTodoApp.HuggingFaceMcp.Application.Interfaces;

public interface IToolRegistry
{
    List<ToolDefinition> GetTools();

    Task<object> ExecuteAsync(string tool, Dictionary<string, object> args);
}
