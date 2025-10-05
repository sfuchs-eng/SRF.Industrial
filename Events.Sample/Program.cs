namespace SRF.Industrial.Events.Sample;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SRF.Industrial.Events.Abstractions;
using SRF.Industrial.Events.Extensions;

public class Program
{
    public static void Main(string[] args)
    {
        // Build the service host
        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddBasicEventsProcessing<SampleEvent>();

        var app = builder.Build();

        // Register event handlers. Could be done from another DI service as well.
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