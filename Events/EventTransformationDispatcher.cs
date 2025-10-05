using Microsoft.Extensions.Logging;
using SRF.Industrial.Events.Abstractions;

namespace SRF.Industrial.Events;

/// <summary>
/// an <see cref="EventDispatcher"/> that executes the handler sequentially,
/// allowing each handler to modify the event/context for the next handler
/// </summary>
public class EventTransformationDispatcher(
    IEventQueue eventQueue,
    ILogger<EventTransformationDispatcher> logger,
    ILogger<EventDispatcher> baseLogger
        ) : EventDispatcher(eventQueue, baseLogger)
{
    private readonly ILogger<EventTransformationDispatcher> logger = logger;

    /// <summary>
    /// Invoke each handler in sequence, allowing each handler to modify the event/context for the next handler.
    /// </summary>
    protected override async Task InvokeHandlersAsync(IEventContext ctx, IEventHandler[] handlers, CancellationToken stoppingToken)
    {
        foreach (var h in handlers)
        {
            try
            {
                await h.HandleAsync(ctx, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Handler {HandlerType} failed to handle event {EventId} of type {EventType} in {QueueName}",
                    h.GetType().FullName, ctx.Id, ctx.Type, Name);
            }
        }
    }
}
