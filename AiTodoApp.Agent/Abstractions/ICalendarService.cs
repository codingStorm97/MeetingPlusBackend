using AiTodoApp.Application.DTOs;

namespace AiTodoApp.Agent.Abstractions;

public interface ICalendarService
{
    Task CreateAsync(string userId, ConversationEventDto calendarEvent, CancellationToken cancellationToken);
}
