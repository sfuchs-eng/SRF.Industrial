using Microsoft.Extensions.Logging;
using SRF.Industrial.Events.Abstractions;

namespace SRF.Industrial.Events;

/// <summary>
/// Base class for event handlers registered with an <see cref="IEventQueue"/>.
/// The abstract method <see cref="HandleAsync(TEvent, CancellationToken)"/> must be implemented to handle events of type <typeparamref name="TEvent"/>.
/// It's only called for events of the correct type. The property <see cref="ThrowOnUnsupportedEventType"/> can be set to true to throw an exception
/// if an event of a different type is passed to <see cref="HandleAsync(IEventContext, CancellationToken)"/>.
/// The constructor requires the queue key to register with, an <see cref="IEventQueueProvider"/> to get the queue from, and a logger instance.
/// The event handler is automatically registered with the queue obtained from the provider.
/// </summary>
/// <typeparam name="TEvent">Type of event to be handled.</typeparam>
public abstract class EventHandlerBase<TEvent> : IEventHandler where TEvent : IEventContext
{
    protected readonly IEventQueue eventQueue;
    protected readonly ILogger<EventHandlerBase<TEvent>> logger;

    protected bool ThrowOnUnsupportedEventType { get; set; } = false;

    public EventHandlerBase(
        string queueKey,
        IEventQueueProvider eventQueueProvider,
        ILogger<EventHandlerBase<TEvent>> logger
    )
    {
        this.eventQueue = eventQueueProvider.GetQueue(queueKey);
        this.eventQueue.Register(this);
        this.logger = logger;
    }

    public virtual Task HandleAsync(IEventContext eventContext, CancellationToken cancellationToken)
    {
        if (eventContext is TEvent typedEventContext)
        {
            return HandleAsync(typedEventContext, cancellationToken);
        }
        else
        {
            if (ThrowOnUnsupportedEventType)
                throw new ArgumentException($"Invalid event context type: {eventContext.GetType().FullName}, expected: {typeof(TEvent).FullName}");
            else
                logger.LogTrace("Ignoring invalid event context type: {EventContextType}, expected: {ExpectedEventContextType}", eventContext.GetType().FullName, typeof(TEvent).FullName);
        }
        return Task.CompletedTask;
    }

    public abstract Task HandleAsync(TEvent eventContext, CancellationToken cancellationToken);
}
