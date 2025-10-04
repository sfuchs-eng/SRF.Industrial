namespace SRF.Industrial.Events.Abstractions;

public interface IEventContextFactory
{
    public Task<IEventContext> CreateEventContextAsync(IEventReceiver eventReceiver, IEvent eventReceived);
}
