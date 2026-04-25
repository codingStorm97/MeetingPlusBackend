using AiTodoApp.Agent.Abstractions;
using AiTodoApp.Application.DTOs;

namespace AiTodoApp.Agent.Services;

public class AgentService : AiTodoApp.Agent.Abstractions.IAgentService, AiTodoApp.Application.Interfaces.IAgentService
{
    private readonly IVectorRepository _vectorRepo;
    private readonly AiTodoApp.Application.Interfaces.IConversationService _llmService;
    private readonly ICalendarService _calendarService;

    public AgentService(
        IVectorRepository vectorRepo,
        AiTodoApp.Application.Interfaces.IConversationService llmService,
        ICalendarService calendarService)
    {
        _vectorRepo = vectorRepo;
        _llmService = llmService;
        _calendarService = calendarService;
    }

    public async Task<ConversationAnswerDto> HandleAsync(
        Guid conversationId,
        string userId,
        string question,
        string context,
        CancellationToken cancellationToken)
    {
        var enrichedContext = context;
        var vectorQuery = question;

        if (!string.IsNullOrWhiteSpace(question))
        {
            vectorQuery = await _llmService.GetVectorSearchQuery(
                conversationId,
                question,
                cancellationToken);

            var results = await _vectorRepo.SearchAsync(userId, vectorQuery, cancellationToken);
            var vectorText = string.Join('\n', results.Select(r => r.Text));
            enrichedContext =
                $"Original user question:\n{question}\n\n" +
                $"Vector search query:\n{vectorQuery}\n\n" +
                $"Context:\n{context}\n\n" +
                $"Retrieved knowledge:\n{vectorText}".Trim();
        }

        var response = await _llmService.AskAsync(
            conversationId,
            question,
            enrichedContext,
            cancellationToken);

        if (string.Equals(response.Intent, "create_event", StringComparison.OrdinalIgnoreCase) && response.Event is not null)
        {
            await _calendarService.CreateAsync(userId, response.Event, cancellationToken);
        }

        return response;
    }
}
