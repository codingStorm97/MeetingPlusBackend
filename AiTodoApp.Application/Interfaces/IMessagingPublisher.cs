using AiTodoApp.Messaging.Contracts;

namespace AiTodoApp.Application.Interfaces;

public interface IMessagingPublisher
{
    Task PublishFileUploadedAsync(FileUploadedNotification notification, CancellationToken cancellationToken = default);
    Task PublishJobStatusChangedAsync(JobStatusChangedMessage message, CancellationToken cancellationToken = default);
}
