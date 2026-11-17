using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using OCID.Messaging.Configuration;
using OCID.Messaging.Interfaces;
using OCID.Messaging.Models;
using OCID.Messaging.Serialization;

namespace OCID.Messaging.Subscriptions;

/// <summary>Binds a topic + consumer group to an event type and its handler. Provider-agnostic.</summary>
public abstract class EventSubscription
{
    protected EventSubscription(string topic, string? consumerGroup, string eventType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topic);
        ArgumentException.ThrowIfNullOrWhiteSpace(eventType);
        Topic = topic;
        ConsumerGroup = consumerGroup;
        EventType = eventType;
    }

    public string Topic { get; }

    /// <summary>Null means <see cref="MessagingOptions.DefaultConsumerGroup"/>.</summary>
    public string? ConsumerGroup { get; }

    public string EventType { get; }

    /// <summary>
    /// True when the message carries this subscription's event type, or carries no type at all
    /// (so several event types can share one topic).
    /// </summary>
    public bool Matches(RawMessage message) =>
        !message.Headers.TryGetValue(MessageHeaders.EventType, out var type)
        || string.Equals(type, EventType, StringComparison.Ordinal);

    public abstract Task DispatchAsync(IServiceProvider services, RawMessage message, CancellationToken cancellationToken);
}

public sealed class EventSubscription<TEvent, THandler> : EventSubscription
    where THandler : class, IEventHandler<TEvent>
{
    public EventSubscription(string topic, string? consumerGroup = null, string? eventType = null)
        : base(topic, consumerGroup, eventType ?? typeof(TEvent).Name)
    {
    }

    public override async Task DispatchAsync(IServiceProvider services, RawMessage message, CancellationToken cancellationToken)
    {
        var payload = EventSerializer.Deserialize<TEvent>(message.Body)
            ?? throw new InvalidOperationException($"Event body on topic '{message.Topic}' deserialized to null for {typeof(TEvent).Name}.");

        message.Headers.TryGetValue(MessageHeaders.EventId, out var eventId);
        message.Headers.TryGetValue(MessageHeaders.CorrelationId, out var correlationId);

        var occurredAt = message.Headers.TryGetValue(MessageHeaders.OccurredAt, out var occurred)
            && DateTimeOffset.TryParse(occurred, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed)
                ? parsed
                : message.EnqueuedAt;

        var received = new ReceivedEvent<TEvent>(
            payload,
            eventId ?? string.Empty,
            EventType,
            correlationId,
            occurredAt,
            message.Topic,
            message.PartitionId,
            message.Key,
            message.Offset);

        var handler = services.GetRequiredService<THandler>();
        await handler.HandleAsync(received, cancellationToken).ConfigureAwait(false);
    }
}
