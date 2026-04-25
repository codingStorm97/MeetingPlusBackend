using AiTodoApp.HuggingFaceMcp.Application.Queries;
using AiTodoApp.HuggingFaceMcp.Application.Interfaces;
using AiTodoApp.HuggingFaceMcp.Application.Services;
using MediatR;

namespace AiTodoApp.HuggingFaceMcp.Application.Handlers;

public sealed class GetConversationResultQueryHandler(
    IConversationAnswerService conversationAnswerService,
    IConversationHistoryStore conversationHistoryStore)
    : IRequestHandler<GetConversationResultQuery, ConversationResultDto>
{
    public async Task<ConversationResultDto> Handle(
        GetConversationResultQuery request,
        CancellationToken cancellationToken)
    {
        var result = await conversationAnswerService.GetAnswerAsync(
            request.ConversationId,
            request.Question,
            request.Context,
            cancellationToken);

        await conversationHistoryStore.AppendAsync(
            request.ConversationId,
            request.Question,
            result,
            cancellationToken);

        return new ConversationResultDto(request.ConversationId, result);
    }
}
