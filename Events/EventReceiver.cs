using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SRF.Industrial.Events.Abstractions;

namespace SRF.Industrial.Events;

/// <summary>
/// Base class for event receivers, deriving from <see cref="BackgroundService"/> and implementing <see cref="IEventReceiver"/>.
/// Override ExecuteAsync to implement the event receiving and <see cref="IEvent"/> creation logic.
/// Then call <see cref="EnqueEventForProcessingAsync(IEvent, CancellationToken)"/> to create the <see cref="IEventContext"/>
/// and enqueue it to the first <see cref="IEventHandlingDispatcher"/> in the context's queue list.
/// </summary>
public abstract class EventReceiver(
    IEventContextFactory eventContextFactory,
    ILogger<EventReceiver> logger
    ) : BackgroundService, IEventReceiver
{
    protected readonly ILogger<EventReceiver> logger = logger;
    public IEventContextFactory EventContextFactory { get; init; } = eventContextFactory ?? throw new ArgumentNullException(nameof(eventContextFactory));

    /// <summary>
    /// Creates the <see cref="IEventContext"/> using the configured <see cref="IEventContextFactory"/>
    /// and en-queues the event context to the first <see cref="IEventHandlingDispatcher"/> in the context's queue list.
    /// </summary>
    /// <param name="evt">the <see cref="IEvent"/></param>
    /// <param name="token"></param>
    /// <returns></returns>
    protected async Task EnqueEventForProcessingAsync(IEvent evt, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(evt);

        try
        {
            var ctx = await EventContextFactory.CreateEventContextAsync(this, evt);

            logger.LogTrace("Processing event {EventId} of type {EventType}, time-stamp {TimeStamp}", ctx.Id, ctx.Type, ctx.Timestamp);

            if (!ctx.ProcessingSteps.TryDequeue(out var q))
            {
                logger.LogWarning("No handling queues configured for event {EventId} of type {EventType}", ctx.Id, ctx.Type);
                return;
            }
            q.EnqueueEvent(ctx);
            logger.LogTrace("Enqueued event {EventId} of type {EventType} to first handling queue {QueueName}", ctx.Id, ctx.Type, q.Name);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Event lost: failed to enqueue event of type {EventType} using {IEventContextFactory} of type {EventContextFactoryType}",
                evt.GetType().FullName, nameof(IEventContextFactory), EventContextFactory.GetType().FullName);
        }
    }
}
