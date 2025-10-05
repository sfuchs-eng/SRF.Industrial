using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SRF.Industrial.Events.Abstractions;

namespace SRF.Industrial.Events;

/// <summary>
/// Base class for event dispatchers, deriving from <see cref="BackgroundService"/> and implementing <see cref="IEventHandlingDispatcher"/>.
/// </summary>
public abstract class EventDispatcher(
    IEventQueue eventQueue,
    ILogger<EventDispatcher> logger
    ) : BackgroundService, IEventHandlingDispatcher
{
    public string Name => this.GetType()?.FullName ?? nameof(EventDispatcher);

    private readonly IEventQueue eventQueue = eventQueue;
    private readonly ILogger<EventDispatcher> logger = logger;

    protected virtual void FinishAndPassOnEvent(IEventContext eventContext)
    {
        ArgumentNullException.ThrowIfNull(eventContext);

        if (!eventContext.ProcessingSteps.TryDequeue(out var q))
        {
            logger.LogWarning("No further handling queues configured for event {EventId} of type {EventType}", eventContext.Id, eventContext.Type);
            return;
        }
        q.EnqueueEvent(eventContext);
        logger.LogTrace("Passed on event {EventId} of type {EventType} to next handling queue {QueueName}", eventContext.Id, eventContext.Type, q.Name);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var ctx = await eventQueue.DequeueEventAsync(stoppingToken);
                if (ctx == null)
                    continue;

                var handlers = eventQueue.Handlers;

                try
                {
                    await InvokeHandlersAsync(ctx, handlers, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "One or more handlers failed to handle event {EventId} of type {EventType} in {QueueName}. Passing on event to next queue.",
                        ctx.Id, ctx.Type, Name);
                }

                FinishAndPassOnEvent(ctx);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // shutting down
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error in {QueueName}", Name);
            }
        }
    }

    protected abstract Task InvokeHandlersAsync(IEventContext ctx, IEventHandler[] handlers, CancellationToken stoppingToken);
}
