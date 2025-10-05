using Microsoft.Extensions.Logging;
using SRF.Industrial.Events.Abstractions;

namespace SRF.Industrial.Events;

public class EventContextFactory<TEvent>(
        IEventQueueProvider eventQueueProvider,
        ILogger<EventContextFactory<TEvent>> logger
    ) : IEventContextFactory<TEvent> where TEvent : class, IEvent
{
    private readonly IEventQueueProvider eventQueueProvider = eventQueueProvider;
    private readonly ILogger<EventContextFactory<TEvent>> logger = logger;

    public virtual Func<IEvent, object?> IdGetter { get; init; } = (evt) => null;
    public virtual Func<IEvent, object?> TypeGetter { get; init; } = (evt) => evt.GetType();
    public virtual Func<IEvent, DateTimeOffset> TimestampGetter { get; init; } = (evt) => DateTimeOffset.UtcNow;

    public async Task<IEventContext> CreateEventContextAsync(IEventReceiver eventReceiver, IEvent eventReceived)
    {
        return await CreateEventContextAsync(
                eventReceiver,
                eventReceived as TEvent ?? throw new InvalidOperationException($"Event is not of expected type {typeof(TEvent).FullName}")
            );
    }

    public Task<IEventContext<TEvent>> CreateEventContextAsync(IEventReceiver eventReceiver, TEvent eventReceived)
    {
        var ctx = new EventContext<TEvent>
        {
            Event = eventReceived,
            Id = IdGetter(eventReceived),
            Type = TypeGetter(eventReceived),
            Source = eventReceiver.GetType(),
            Timestamp = TimestampGetter(eventReceived)
        };
        foreach ( var q in eventQueueProvider.DefaultProcessingSequence )
            ctx.ProcessingSteps.Enqueue(q);
        return Task.FromResult((IEventContext<TEvent>)ctx);
    }
}
