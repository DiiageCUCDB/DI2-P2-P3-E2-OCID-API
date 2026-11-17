using Microsoft.Extensions.Logging;
using OCID.Messaging.Interfaces;
using OCID.Messaging.Models;

namespace OCID.Messaging.Providers;

/// <summary>Registered when messaging is disabled: drops every event (logged at debug level).</summary>
public sealed class NullEventPublisher : IEventPublisher
{
    private readonly ILogger<NullEventPublisher> _logger;

    public NullEventPublisher(ILogger<NullEventPublisher> logger) => _logger = logger;

    public Task PublishAsync<TEvent>(string topic, TEvent payload, PublishOptions? options = null, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Messaging disabled: dropped {EventType} for topic {Topic}", typeof(TEvent).Name, topic);
        return Task.CompletedTask;
    }

    public Task PublishBatchAsync<TEvent>(string topic, IEnumerable<TEvent> payloads, PublishOptions? options = null, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Messaging disabled: dropped batch of {EventType} for topic {Topic}", typeof(TEvent).Name, topic);
        return Task.CompletedTask;
    }
}
