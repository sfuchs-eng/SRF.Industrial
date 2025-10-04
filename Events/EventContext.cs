using System.Collections.Concurrent;
using SRF.Industrial.Events.Abstractions;

namespace SRF.Industrial.Events;

/// <summary>
/// Basic implementation of IEventContext
/// </summary>
public class EventContext : IEventContext
{
    public object? Id { get; set; }

    public object? Type { get; set; }

    public object? Source { get; set; }

    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    public ConcurrentQueue<IEventHandlingDispatcher> ProcessingSteps { get; } = [];

    public required IEvent Event { get; init; }
}