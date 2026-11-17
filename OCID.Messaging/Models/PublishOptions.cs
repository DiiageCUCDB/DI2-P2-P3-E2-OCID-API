namespace OCID.Messaging.Models;

public sealed record PublishOptions
{
    /// <summary>Partition key: events sharing a key are delivered in order to the same partition.</summary>
    public string? Key { get; init; }

    /// <summary>Logical event type used by consumers to route the message. Defaults to <c>typeof(TEvent).Name</c>.</summary>
    public string? EventType { get; init; }

    public string? CorrelationId { get; init; }

    /// <summary>Extra headers/properties sent along with the event.</summary>
    public IReadOnlyDictionary<string, string>? Properties { get; init; }
}
