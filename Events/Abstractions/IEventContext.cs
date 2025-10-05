namespace SRF.Industrial.Events.Abstractions;

public interface IEventContext
{
    IEvent GenericEvent { get; }
    object? Id { get; }
    object? Type { get; }
    object? Source { get; }
    DateTimeOffset Timestamp { get; }
    System.Collections.Concurrent.ConcurrentQueue<IEventQueue> ProcessingSteps { get; }
}

public interface IEventContext<TEvent> : IEventContext where TEvent : class, IEvent
{
    TEvent Event { get; set; }
}