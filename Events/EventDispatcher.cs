using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SRF.Industrial.Events.Abstractions;

namespace SRF.Industrial.Events;

/// <summary>
/// Base class for event dispatchers, deriving from <see cref="BackgroundService"/> and implementing <see cref="IEventHandlingDispatcher"/>.
/// </summary>
public abstract class EventDispatcher : BackgroundService, IEventHandlingDispatcher
{
    public string Name => $"{eventQueue.Name}-{this.GetType().Name}" ?? $"{this.GetType().FullName} without associated Queue!";

    private readonly IEventQueue eventQueue;
    private readonly ILogger<EventDispatcher> logger;

    public EventDispatcher(
        IEventQueue eventQueue,
        ILogger<EventDispatcher> logger
    )
    {
        this.eventQueue = eventQueue;
        this.logger = logger;
    }

    protected virtual void FinishAndPassOnEvent(IEventContext eventContext)
    {
        ArgumentNullException.ThrowIfNull(eventContext);

        if (!eventContext.ProcessingSteps.TryDequeue(out var q))
            return;
        q.EnqueueEvent(eventContext);
        logger.LogTrace("Passed on event {EventId} of type {EventType} to next handling queue {QueueName}", eventContext.Id, eventContext.Type, q.Name);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting event dispatcher for queue {QueueName}", Name);
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
        logger.LogTrace("Stopping event dispatcher for queue {QueueName}", Name);
    }

    protected abstract Task InvokeHandlersAsync(IEventContext ctx, IEventHandler[] handlers, CancellationToken stoppingToken);
}
