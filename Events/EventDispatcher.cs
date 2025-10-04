using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SRF.Industrial.Events.Abstractions;

namespace SRF.Industrial.Events;

/// <summary>
/// Base class for event dispatchers, deriving from <see cref="BackgroundService"/> and implementing <see cref="IEventHandlingDispatcher"/>.
/// </summary>
public abstract class EventDispatcher(ILogger<EventDispatcher> logger) : BackgroundService, IEventHandlingDispatcher
{
    public string Name => this.GetType()?.FullName ?? nameof(EventDispatcher);

    /// <summary>
    /// Queue of incoming events to be processed.
    /// Thread/concurrency safe, single reader, multiple writers.
    /// </summary>
    protected readonly Channel<IEventContext> EventQueue = Channel.CreateUnbounded<IEventContext>(new UnboundedChannelOptions()
    {
        SingleReader = true,
        SingleWriter = false
    });

    public virtual void EnqueueEvent(IEventContext eventContext)
    {
        if (!EventQueue.Writer.TryWrite(eventContext))
            logger.LogWarning("Event loss: failed to enqueue event {EventId} of type {EventType} in {QueueName}", eventContext.Id, eventContext.Type, Name);
    }

    protected virtual async Task<IEventContext?> DequeueEventAsync(CancellationToken token)
    {
        return await EventQueue.Reader.ReadAsync(token);
    }

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

    protected readonly Lock handlerListLock = new();

    /// <summary>
    /// The list of registered event handlers.
    /// Lock handlerListLock must be held when accessing this list.
    /// </summary>
    protected readonly LinkedList<IEventHandler> Handlers = [];

    private readonly ILogger<EventDispatcher> logger = logger;

    public void Register(IEventHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        lock (handlerListLock)
        {
            if (!Handlers.Contains(handler))
                Handlers.AddLast(handler);
            else
                throw new InvalidOperationException($"Handler {handler.GetType().FullName} already registered in {Name}");
        }
    }

    public void Unregister(IEventHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        lock (handlerListLock)
        {
            Handlers.Remove(handler);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var ctx = await DequeueEventAsync(stoppingToken);
                if (ctx == null)
                    continue;

                IEventHandler[] handlers;
                lock (handlerListLock)
                {
                    handlers = [.. Handlers];
                }

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
