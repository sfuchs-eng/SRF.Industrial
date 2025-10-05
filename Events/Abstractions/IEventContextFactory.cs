namespace SRF.Industrial.Events.Abstractions;

public interface IEventContextFactory
{
    public Task<IEventContext> CreateEventContextAsync(IEventReceiver eventReceiver, IEvent eventReceived);
}

public interface IEventContextFactory<TEvent> : IEventContextFactory where TEvent : class, IEvent
{
    public Task<IEventContext<TEvent>> CreateEventContextAsync(IEventReceiver eventReceiver, TEvent eventReceived);
}