using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SRF.Industrial.Events.Abstractions;

namespace SRF.Industrial.Events.Extensions;

public static class HostingExtensions
{
    /// <summary>
    /// Adds default basic implementations for event processing:
    /// - EventContextFactory
    /// - EventTransformationDispatcher
    /// - EventProcessingDispatcher
    /// </summary>
    public static IServiceCollection AddBasicEventsProcessing(this IServiceCollection services)
    {
        services.AddSingleton<IEventContextFactory, EventContextFactory>();
        services.AddHostedService<EventTransformationDispatcher>();
        services.AddHostedService<EventProcessingDispatcher>();

        return services;
    }

    public static IServiceCollection AddEventsProcessing<TReceiver, TContextFactory>(this IServiceCollection services)
        where TContextFactory : class, IEventContextFactory
        where TReceiver : class, IEventReceiver
    {
        services.AddSingleton<IEventContextFactory, TContextFactory>();

        services.AddHostedService<TReceiver>();
        services.AddHostedService<EventTransformationDispatcher>();
        services.AddHostedService<EventProcessingDispatcher>();

        return services;
    }
}

TODO: need an interface type that can be injected and allows retrieval of the Dispatchers to register handlers. Keyed Singletons which are also consumed by the Dispatchers and contain the handler lists?
--> decouple worker (BackgroundService) from queues & lists (KeyedSingleton)
