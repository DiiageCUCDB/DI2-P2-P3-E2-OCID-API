using OCID.Messaging.Interfaces;

namespace OCID.Messaging.Models;

/// <summary>A deserialized event plus its metadata, as given to an <see cref="IEventHandler{TEvent}"/>.</summary>
public sealed record ReceivedEvent<TEvent>(
    TEvent Payload,
    string EventId,
    string EventType,
    string? CorrelationId,
    DateTimeOffset OccurredAt,
    string Topic,
    string? PartitionId,
    string? Key,
    string? Offset);
