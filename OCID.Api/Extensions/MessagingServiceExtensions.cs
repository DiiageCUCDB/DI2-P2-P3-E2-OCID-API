using OCID.Messaging.Configuration;
using OCID.Messaging.Interfaces;
using OCID.Messaging.Providers;
using OCID.Messaging.Providers.AzureEventHub;
using OCID.Messaging.Subscriptions;

namespace OCID.Api.Extensions;

public static class MessagingServiceExtensions
{
    /// <summary>
    /// Registers messaging from the "Messaging" configuration section: <see cref="IEventPublisher"/>
    /// and, when subscriptions were declared, the background consumer.
    /// With <c>Messaging:Enabled=false</c> a no-op publisher is registered instead.
    /// </summary>
    /// <example>
    /// <code>
    /// services.AddMessaging(config, m => m.Subscribe&lt;UserCreated, UserCreatedHandler&gt;("users"));
    /// </code>
    /// </example>
    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<MessagingBuilder>? configure = null)
    {
        var section = configuration.GetSection(MessagingOptions.SectionName);
        var options = section.Get<MessagingOptions>() ?? new MessagingOptions();

        services.AddOptions<MessagingOptions>()
            .Bind(section)
            .Validate(
                o => !o.Enabled || o.Provider != MessagingProvider.AzureEventHub || !string.IsNullOrWhiteSpace(o.AzureEventHub.ConnectionString),
                "Messaging:AzureEventHub:ConnectionString must be set when messaging is enabled.")
            .ValidateOnStart();

        configure?.Invoke(new MessagingBuilder(services));

        if (!options.Enabled)
        {
            services.AddSingleton<IEventPublisher, NullEventPublisher>();
            return services;
        }

        switch (options.Provider)
        {
            case MessagingProvider.AzureEventHub:
                services.AddSingleton<IEventPublisher, AzureEventHubPublisher>();
                services.AddHostedService<AzureEventHubConsumerService>();
                break;

            default:
                throw new NotSupportedException($"Messaging provider '{options.Provider}' is not implemented yet.");
        }

        return services;
    }
}

public sealed class MessagingBuilder
{
    private readonly IServiceCollection _services;

    internal MessagingBuilder(IServiceCollection services) => _services = services;

    /// <summary>
    /// Subscribes <typeparamref name="THandler"/> to events of type <typeparamref name="TEvent"/> on <paramref name="topic"/>.
    /// Several event types can share a topic; they are routed by the event-type header written by the publisher.
    /// </summary>
    /// <param name="consumerGroup">Null uses <c>Messaging:DefaultConsumerGroup</c>.</param>
    /// <param name="eventType">Null uses <c>typeof(TEvent).Name</c>.</param>
    public MessagingBuilder Subscribe<TEvent, THandler>(string topic, string? consumerGroup = null, string? eventType = null)
        where THandler : class, IEventHandler<TEvent>
    {
        _services.AddScoped<THandler>();
        _services.AddSingleton<EventSubscription>(new EventSubscription<TEvent, THandler>(topic, consumerGroup, eventType));
        return this;
    }
}
