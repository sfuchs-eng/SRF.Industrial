using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SRF.Industrial.Events.Abstractions;

namespace SRF.Industrial.Events.Extensions;

public static class HostingExtensions
{
    /// <summary>
    /// Adds default basic implementations for event processing:
    /// - EventQueue (2x, keyed: "EventTransformation", "EventProcessing")
    /// - EventQueueProvider (with above default processing sequence)
    /// - EventContextFactory
    /// - EventTransformationDispatcher
    /// - EventProcessingDispatcher
    /// </summary>
    public static IServiceCollection AddBasicEventsProcessing<TEvent>(this IServiceCollection services) where TEvent : class, IEvent
    {
        var transformQueueKey = "EventTransformation";
        var processingQueueKey = "EventProcessing";

        services.AddSingleton<IEventQueueProvider,EventQueueProvider>(
            (s) => new EventQueueProvider(
                s,
                [transformQueueKey, processingQueueKey],
                s.GetRequiredService<ILogger<EventQueueProvider>>()
            ));
            
        services.AddSingleton<IEventContextFactory<TEvent>, EventContextFactory<TEvent>>();

        services.AddKeyedSingleton<IEventQueue, EventQueue>(transformQueueKey);
        services.AddKeyedSingleton<IEventQueue, EventQueue>(processingQueueKey);

        services.AddHostedService(
            (s) => new EventTransformationDispatcher(
                s.GetRequiredKeyedService<IEventQueue>(transformQueueKey),
                s.GetRequiredService<ILogger<EventTransformationDispatcher>>(),
                s.GetRequiredService<ILogger<EventDispatcher>>()
            )
        ); 
        services.AddHostedService(
            (s) => new EventProcessingDispatcher(
                s.GetRequiredKeyedService<IEventQueue>(processingQueueKey),
                s.GetRequiredService<ILogger<EventProcessingDispatcher>>(),
                s.GetRequiredService<ILogger<EventDispatcher>>()
            )
        );

        return services;
    }
}
