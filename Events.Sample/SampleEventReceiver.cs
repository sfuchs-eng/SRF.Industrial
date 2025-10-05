using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SRF.Industrial.Events.Abstractions;

namespace SRF.Industrial.Events.Sample;

public class SampleEventReceiver(
    IHostApplicationLifetime applicationLifetime,
    IEventContextFactory eventContextFactory,
    ILogger<SampleEventReceiver> logger
        ) : EventReceiver(eventContextFactory, logger)
{
    private readonly IHostApplicationLifetime applicationLifetime = applicationLifetime;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        /// <summary>
        /// Simulate receiving events from an event source, e.g. via MQTT, WebSocket, OPC UA, ...
        /// </summary>
        var receivedEvents = Enumerable.Range(1, 1000)
            .Select(_ => new SampleEvent()
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                Message = $"It's all the same except {Random.Shared.Next(0, 1000)}"
            });

        // this would normally last until the application gets terminated.
        foreach (var receivedEvent in receivedEvents)
        {
            if (stoppingToken.IsCancellationRequested)
                break;

            await this.EnqueEventForProcessingAsync(receivedEvent, stoppingToken);
        }

        // how to wait until all events are processed fully?
        await Task.Delay(5000, stoppingToken);

        applicationLifetime.StopApplication();
    }
}
