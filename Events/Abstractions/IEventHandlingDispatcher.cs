namespace SRF.Industrial.Events.Abstractions;

public interface IEventHandlingDispatcher
{
    /// <summary>
    /// For identification/logging purposes
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Enqueue the event context for processing in this handling queue
    /// Must be non-blocking
    /// </summary>
    /// <param name="eventContext"></param>
    public void EnqueueEvent(IEventContext eventContext);
}
