namespace AiTodoApp.HuggingFaceMcp.Application.Models;

public sealed class ToolExecuteRequest
{
    public string Tool { get; init; } = string.Empty;
    public Dictionary<string, object> Arguments { get; init; } = [];
}
