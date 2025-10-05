namespace SRF.Industrial.Events.Sample;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SRF.Industrial.Events.Abstractions;
using SRF.Industrial.Events.Extensions;

public class Program
{
    public static void Main(string[] args)
    {
        // Build the service host
        var builder = Host.CreateApplicationBuilder(args);
        builder.Logging.AddSimpleConsole();

        // Add event processing services
        // If you have different Event types, either configure all services directly here,
        // or create separate service collections for each event type and merge them into the main service collection
        builder.Services.AddBasicEventsProcessing<SampleEvent>();
        builder.Services.AddHostedService<SampleEventReceiver>();

        var app = builder.Build();

        // Register event handlers. Could be done from another DI service as well and at any time once services are available.
        var queueProvider = app.Services.GetRequiredService<IEventQueueProvider>();
        queueProvider.GetQueue("EventTransformation").Register(
            new SampleTransformationHandler(
                app.Services.GetRequiredService<Microsoft.Extensions.Logging.ILogger<EventHandlerBase<IEventContext<SampleEvent>>>>()
            ));
        queueProvider.GetQueue("EventProcessing").Register(
            new SampleProcessingHandler(
                app.Services.GetRequiredService<Microsoft.Extensions.Logging.ILogger<EventHandlerBase<IEventContext<SampleEvent>>>>()
            ));

        app.Run();
    }
}