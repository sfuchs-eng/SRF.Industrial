using System.Collections.Concurrent;
using SRF.Industrial.Events.Abstractions;

namespace SRF.Industrial.Events;

/// <summary>
/// Basic implementation of IEventContext
/// </summary>
public class EventContext<TEvent> : IEventContext<TEvent> where TEvent : class, IEvent
{
    public object? Id { get; set; }

    public object? Type { get; set; }

    public object? Source { get; set; }

    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    public ConcurrentQueue<IEventQueue> ProcessingSteps { get; } = [];

    public virtual required TEvent Event { get; set; }

    IEvent IEventContext.GenericEvent
    {
        get => Event;
    }
}
