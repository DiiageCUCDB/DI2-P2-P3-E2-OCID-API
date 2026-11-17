using OCID.Messaging.Models;

namespace OCID.Messaging.Interfaces;

/// <summary>
/// Handles one type of event received from a topic. Register it with
/// <see cref="MessagingBuilder.Subscribe{TEvent, THandler}"/>. Handlers are resolved from a new DI scope for every message.
/// </summary>
public interface IEventHandler<TEvent>
{
    Task HandleAsync(ReceivedEvent<TEvent> message, CancellationToken cancellationToken);
}
