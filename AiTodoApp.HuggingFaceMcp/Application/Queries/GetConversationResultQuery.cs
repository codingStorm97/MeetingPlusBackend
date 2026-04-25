using MediatR;

namespace AiTodoApp.HuggingFaceMcp.Application.Queries;

public sealed record GetConversationResultQuery(
    string ConversationId,
    string Question,
    string Context) : IRequest<ConversationResultDto>;

public sealed record ConversationResultDto(string ConversationId, string Result);
