using OCID.Messaging.Models;

namespace OCID.Messaging.Interfaces;

/// <summary>
/// Provider-agnostic way to publish events. The current implementation targets Azure Event Hubs;
/// a Kafka implementation can be added later without touching the callers.
/// A <c>topic</c> is an Event Hub name (Azure) or a topic name (Kafka).
/// </summary>
public interface IEventPublisher
{
    /// <summary>Serializes <paramref name="payload"/> as JSON and publishes it to <paramref name="topic"/>.</summary>
    Task PublishAsync<TEvent>(string topic, TEvent payload, PublishOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Publishes several events to <paramref name="topic"/>, split into as many provider batches as needed.</summary>
    Task PublishBatchAsync<TEvent>(string topic, IEnumerable<TEvent> payloads, PublishOptions? options = null, CancellationToken cancellationToken = default);
}
