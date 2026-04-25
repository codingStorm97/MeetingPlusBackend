using System.Collections.Concurrent;
using System.Threading.Channels;
using AiTodoApp.Messaging.Contracts;

namespace AiTodoApp.Api.Services;

public class NotificationStreamService
{
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, Channel<object>>> _subscriptions = new();

    public NotificationSubscription Subscribe(Guid userId, CancellationToken cancellationToken)
    {
        var channel = Channel.CreateUnbounded<object>();
        var subscriptionId = Guid.NewGuid();
        var userSubscriptions = _subscriptions.GetOrAdd(userId, static _ => new ConcurrentDictionary<Guid, Channel<object>>());
        userSubscriptions[subscriptionId] = channel;

        cancellationToken.Register(() =>
        {
            if (_subscriptions.TryGetValue(userId, out var existing))
            {
                existing.TryRemove(subscriptionId, out _);
                if (existing.IsEmpty)
                {
                    _subscriptions.TryRemove(userId, out _);
                }
            }

            channel.Writer.TryComplete();
        });

        return new NotificationSubscription(channel.Reader);
    }

    public void PublishFileUploaded(FileUploadedNotification message)
    {
        Publish(message.UserId, message);
    }

    public void PublishJobStatusChanged(JobStatusChangedMessage message)
    {
        Publish(message.UserId, message);
    }

    private void Publish(Guid userId, object payload)
    {
        if (!_subscriptions.TryGetValue(userId, out var userSubscriptions))
        {
            return;
        }

        foreach (var subscription in userSubscriptions.Values)
        {
            subscription.Writer.TryWrite(payload);
        }
    }
}

public sealed class NotificationSubscription
{
    public NotificationSubscription(ChannelReader<object> reader)
    {
        Reader = reader;
    }

    public ChannelReader<object> Reader { get; }
}
