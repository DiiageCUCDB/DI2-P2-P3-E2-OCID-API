namespace OCID.Messaging.Models;

/// <summary>Provider-neutral view of a received message, before deserialization.</summary>
/// <param name="Offset">Position in the partition (sequence number for Event Hubs, offset for Kafka).</param>
public sealed record RawMessage(
    string Topic,
    string? PartitionId,
    string? Key,
    BinaryData Body,
    IReadOnlyDictionary<string, string> Headers,
    DateTimeOffset EnqueuedAt,
    string? Offset);
