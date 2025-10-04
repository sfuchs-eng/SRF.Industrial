using SRF.Industrial.Events.Abstractions;

namespace SRF.Industrial.Events;

public class EventContextFactory : IEventContextFactory
{
    public virtual Func<IEvent, object?> IdGetter { get; init; } = (evt) => null;
    public virtual Func<IEvent, object?> TypeGetter { get; init; } = (evt) => evt.GetType();
    public virtual Func<IEvent, DateTimeOffset> TimestampGetter { get; init; } = (evt) => DateTimeOffset.UtcNow;

    public virtual Task<IEventContext> CreateEventContextAsync(IEventReceiver eventReceiver, IEvent eventReceived)
    {
        var ctx = new EventContext
        {
            Event = eventReceived,
            Id = IdGetter(eventReceived),
            Type = TypeGetter(eventReceived),
            Source = eventReceiver.GetType(),
            Timestamp = TimestampGetter(eventReceived)
        };
        return Task.FromResult<IEventContext>((IEventContext)ctx);
    }
}
