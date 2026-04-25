namespace AiTodoApp.HuggingFaceMcp.Application.Models;

public sealed class ToolDefinition
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public ToolInputSchema InputSchema { get; init; } = new();
}

public sealed class ToolInputSchema
{
    public string Type { get; init; } = "object";
    public Dictionary<string, object> Properties { get; init; } = [];
    public List<string> Required { get; init; } = [];
}
