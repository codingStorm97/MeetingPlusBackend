namespace AiTodoApp.HuggingFaceMcp.Application.Models;

public sealed class ToolExecuteResponse
{
    public object? Result { get; init; }
    public ToolError? Error { get; init; }

    public static ToolExecuteResponse Success(object result) => new() { Result = result };

    public static ToolExecuteResponse Failure(string code, string message) =>
        new()
        {
            Error = new ToolError
            {
                Code = code,
                Message = message
            }
        };
}

public sealed class ToolError
{
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
