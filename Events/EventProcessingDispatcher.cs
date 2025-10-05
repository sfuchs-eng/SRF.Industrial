using Microsoft.Extensions.Logging;
using SRF.Industrial.Events.Abstractions;

namespace SRF.Industrial.Events;

public class EventProcessingDispatcher(
    IEventQueue eventQueue,
    ILogger<EventProcessingDispatcher> logger,
    ILogger<EventDispatcher> baseLogger
    ) : EventDispatcher(eventQueue, baseLogger)
{
    private readonly ILogger<EventProcessingDispatcher> logger = logger;

    protected override Task InvokeHandlersAsync(IEventContext ctx, IEventHandler[] handlers, CancellationToken stoppingToken)
    {
        logger.LogTrace("Invoking {HandlerCount} handlers for event {EventId} of type {EventType}", handlers.Length, ctx.Id, ctx.GenericEvent.GetType().FullName);
        return Task.WhenAll(handlers.Select(h => h.HandleAsync(ctx, stoppingToken)));
    }
}
