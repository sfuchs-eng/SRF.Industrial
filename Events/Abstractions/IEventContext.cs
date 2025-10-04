namespace SRF.Industrial.Events.Abstractions;

public interface IEventContext
{
    IEvent Event { get; }
    object? Id { get; }
    object? Type { get; }
    object? Source { get; }
    DateTimeOffset Timestamp { get; }
    System.Collections.Concurrent.ConcurrentQueue<IEventHandlingDispatcher> ProcessingSteps { get; }
}
