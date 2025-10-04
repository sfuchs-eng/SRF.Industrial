namespace SRF.Industrial.Events.Abstractions;

public interface IEventHandler
{
    public Task HandleAsync(IEventContext eventContext, CancellationToken cancellationToken);
}
