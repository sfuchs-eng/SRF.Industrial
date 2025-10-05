using Microsoft.Extensions.Logging;
using SRF.Industrial.Events.Abstractions;

namespace SRF.Industrial.Events.Sample;

public class SampleTransformationHandler : EventHandlerBase<IEventContext<SampleEvent>>
{
    public SampleTransformationHandler(
        ILogger<EventHandlerBase<IEventContext<SampleEvent>>> logger
    ) : base(logger)
    {
        this.ThrowOnUnsupportedEventType = true;
    }

    public override Task HandleAsync(IEventContext<SampleEvent> eventContext, CancellationToken cancellationToken)
    {
        // perform some transformation on the event (or replace the event object entirely by an assignable one)
        eventContext.Event.DerivedProperty = eventContext.Event.Message.Length;

        logger.LogTrace("Transformed event {EventId} created at {EventCreatedAt}: {EventMessage} (DerivedProperty={DerivedProperty})",
            eventContext.Event.Id,
            eventContext.Event.CreatedAt,
            eventContext.Event.Message,
            eventContext.Event.DerivedProperty);

        return Task.CompletedTask;
    }
}
