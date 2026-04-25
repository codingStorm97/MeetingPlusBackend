using AiTodoApp.Agent.Abstractions;
using AiTodoApp.Application.DTOs;

namespace AiTodoApp.Agent.Services;

public sealed class NoOpCalendarService : ICalendarService
{
    public Task CreateAsync(string userId, ConversationEventDto calendarEvent, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
