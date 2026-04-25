using AiTodoApp.Application.Interfaces;
using AiTodoApp.Messaging.Contracts;
using AiTodoApp.Messaging.Messaging.Producers;

namespace AiTodoApp.Messaging.Messaging.Consumers;

public class JobStatusChangedMessageHandler
{
    private readonly IJobService _jobService;
    private readonly UserNotificationProducer _userNotificationProducer;

    public JobStatusChangedMessageHandler(
        IJobService jobService,
        UserNotificationProducer userNotificationProducer)
    {
        _jobService = jobService;
        _userNotificationProducer = userNotificationProducer;
    }

    public async Task HandleAsync(JobStatusChangedMessage message, CancellationToken cancellationToken)
    {
        await _jobService.UpdateJobStatusAsync(
            message.JobId,
            message.Status,
            message.TimeUtc,
            message.Error,
            cancellationToken);

        await _userNotificationProducer.PublishJobStatusChangedAsync(message, cancellationToken);
    }
}
