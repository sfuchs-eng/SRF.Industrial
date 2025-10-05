namespace SRF.Industrial.Events.Abstractions;

/// <summary>
/// A queue that holds event contexts for processing by registered handlers.
/// Ensure to register the queue with an <see cref="IEventQueueProvider"/> implementation.
/// </summary>
public interface IEventQueue
{
    public string Name { get; }

    /// <summary>
    /// Enqueue the event context for processing in this handling queue
    /// Must be non-blocking
    /// </summary>
    /// <param name="eventContext"></param>
    public void EnqueueEvent(IEventContext eventContext);

    public Task<IEventContext> DequeueEventAsync(CancellationToken token);

    IEventHandler[] Handlers { get; }
    public void Register(IEventHandler handler);
    public void Unregister(IEventHandler handler);
}
