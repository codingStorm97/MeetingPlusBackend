using AiTodoApp.Messaging.Contracts;
using AiTodoApp.Messaging.Messaging.Producers;

namespace AiTodoApp.Messaging.Messaging.Consumers;

public class FileUploadedMessageHandler
{
    private readonly EmbeddingJobRequestedProducer _embeddingJobRequestedProducer;
    private readonly UserNotificationProducer _userNotificationProducer;

    public FileUploadedMessageHandler(
        EmbeddingJobRequestedProducer embeddingJobRequestedProducer,
        UserNotificationProducer userNotificationProducer)
    {
        _embeddingJobRequestedProducer = embeddingJobRequestedProducer;
        _userNotificationProducer = userNotificationProducer;
    }

    public async Task HandleAsync(FileUploadedNotification message, CancellationToken cancellationToken)
    {
        var jobRequested = new EmbeddingJobRequestedMessage(
            message.JobId,
            message.UserId,
            message.UserName,
            message.FileName,
            message.RelativePath,
            DateTime.UtcNow);

        await _embeddingJobRequestedProducer.PublishAsync(jobRequested, cancellationToken);
        await _userNotificationProducer.PublishFileUploadedAsync(message, cancellationToken);
    }
}
