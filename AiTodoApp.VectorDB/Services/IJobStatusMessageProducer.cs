using AiTodoApp.Messaging.Contracts;

namespace AiTodoApp.VectorDB.Services;

public interface IJobStatusMessageProducer
{
    Task PublishStatusAsync(JobStatusChangedMessage message, CancellationToken cancellationToken);
}
