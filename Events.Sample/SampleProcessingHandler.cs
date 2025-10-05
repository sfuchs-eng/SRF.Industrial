using Microsoft.Extensions.Logging;
using SRF.Industrial.Events.Abstractions;

namespace SRF.Industrial.Events.Sample;

public class SampleProcessingHandler : EventHandlerBase<IEventContext<SampleEvent>>
{
    public SampleProcessingHandler(
        ILogger<EventHandlerBase<IEventContext<SampleEvent>>> logger
    ) : base(logger)
    {
        // opt for self-registration. Normally some other logic would ensure handler registration.
        this.ThrowOnUnsupportedEventType = true;
    }

    public override Task HandleAsync(IEventContext<SampleEvent> eventContext, CancellationToken cancellationToken)
    {
        logger.LogTrace("Processing event {EventId} created at {EventCreatedAt}: {EventMessage}",
            eventContext.Event.Id,
            eventContext.Event.CreatedAt,
            eventContext.Event.Message);
        return Task.CompletedTask;
    }
}
