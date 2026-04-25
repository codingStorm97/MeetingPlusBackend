using System.Text.Json;
using AiTodoApp.HuggingFaceMcp.Application.Interfaces;
using AiTodoApp.HuggingFaceMcp.Application.Models;
using AiTodoApp.HuggingFaceMcp.Application.Queries;
using MediatR;

namespace AiTodoApp.HuggingFaceMcp.Application.Services;

public sealed class ToolRegistry(IMediator mediator) : IToolRegistry
{
    public List<ToolDefinition> GetTools()
    {
        return
        [
            new ToolDefinition
            {
                Name = "ask_question",
                Description = "Answer user questions using conversation service",
                InputSchema = new ToolInputSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, object>
                    {
                        ["conversationId"] = new { type = "string" },
                        ["question"] = new { type = "string" },
                        ["context"] = new { type = "string" }
                    },
                    Required = ["question"]
                }
            }
        ];
    }

    public Task<object> ExecuteAsync(string tool, Dictionary<string, object> args)
    {
        return ExecuteInternalAsync(tool, args, CancellationToken.None);
    }

    private async Task<object> ExecuteInternalAsync(
        string tool,
        Dictionary<string, object> args,
        CancellationToken cancellationToken)
    {
        return tool switch
        {
            "ask_question" => await ExecuteAskQuestionAsync(args, cancellationToken),
            _ => throw new KeyNotFoundException($"Tool '{tool}' was not found.")
        };
    }

    private async Task<object> ExecuteAskQuestionAsync(Dictionary<string, object> args, CancellationToken cancellationToken)
    {
        var question = GetStringArg(args, "question");
        if (string.IsNullOrWhiteSpace(question))
        {
            throw new ArgumentException("Missing required argument: question");
        }

        var conversationId = GetStringArg(args, "conversationId");
        var context = GetStringArg(args, "context");

        var result = await mediator.Send(
            new GetConversationResultQuery(
                conversationId,
                question,
                context),
            cancellationToken);

        return new
        {
            answer = result.Result,
            confidence = 0.9
        };
    }

    private static string GetStringArg(IReadOnlyDictionary<string, object> args, string key)
    {
        if (!args.TryGetValue(key, out var value) || value is null)
        {
            return string.Empty;
        }

        return value switch
        {
            string text => text,
            JsonElement { ValueKind: JsonValueKind.String } jsonString => jsonString.GetString() ?? string.Empty,
            JsonElement json => json.ToString(),
            _ => value.ToString() ?? string.Empty
        };
    }
}
