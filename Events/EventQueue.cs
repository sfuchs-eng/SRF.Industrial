using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using SRF.Industrial.Events.Abstractions;

namespace SRF.Industrial.Events;

public class EventQueue : IEventQueue
{
    private readonly ILogger<EventQueue> logger;

    public string Name { get; init; }

    /// <summary>
    /// Queue of incoming events to be processed.
    /// Thread/concurrency safe, single reader, multiple writers.
    /// </summary>
    protected readonly Channel<IEventContext> internalQueue = Channel.CreateUnbounded<IEventContext>(new UnboundedChannelOptions()
    {
        SingleReader = true,
        SingleWriter = false
    });

    public virtual void EnqueueEvent(IEventContext eventContext)
    {
        if (!internalQueue.Writer.TryWrite(eventContext))
            logger.LogWarning("Event loss: failed to enqueue event {EventId} of type {EventType} in {QueueName}", eventContext.Id, eventContext.Type, Name);
    }

    public virtual async Task<IEventContext> DequeueEventAsync(CancellationToken token)
    {
        return await internalQueue.Reader.ReadAsync(token);
    }

    protected readonly Lock handlerListLock = new();

    public EventQueue(string nameKey, IEventQueueProvider eventQueueProvider, ILogger<EventQueue> logger)
    {
        this.logger = logger;
        Name = nameKey;
    }

    /// <summary>
    /// The list of registered event handlers.
    /// Lock handlerListLock must be held when accessing this list.
    /// </summary>
    public IEventHandler[] Handlers { get; protected set; } = [];

    public void Register(IEventHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        lock (handlerListLock)
        {
            if (!Handlers.Contains(handler))
                Handlers = [.. Handlers, handler];
            else
                throw new InvalidOperationException($"Handler {handler.GetType().FullName} already registered in {Name}");
        }
    }

    public void Unregister(IEventHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        lock (handlerListLock)
        {
            Handlers = [.. Handlers.Where(h => !h.Equals(handler))];
        }
    }
}
