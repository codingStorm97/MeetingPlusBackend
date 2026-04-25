using AiTodoApp.HuggingFaceMcp.Infrastructure;

namespace AiTodoApp.HuggingFaceMcp.Application.Services;

public interface IConversationAnswerService
{
    Task<string> GetAnswerAsync(string conversationId, string question, string context, CancellationToken cancellationToken);
}

public sealed class ConversationAnswerService(IHuggingFaceContext huggingFaceContext) : IConversationAnswerService
{
    public Task<string> GetAnswerAsync(
        string conversationId,
        string question,
        string context,
        CancellationToken cancellationToken)
    {
        return huggingFaceContext.SendConversationQueryAsync(conversationId, question, context, cancellationToken);
    }
}
