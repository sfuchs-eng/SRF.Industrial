using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SRF.Industrial.Events.Abstractions;

namespace SRF.Industrial.Events;

public class EventQueueProvider : IEventQueueProvider
{
    private readonly ILogger<EventQueueProvider> logger;

    private IEventQueue[] _cachedProcessingSequence;

    public EventQueueProvider(
        IServiceProvider serviceProvider,
        object[] queuesListInDefaultProcessingSequence,
        ILogger<EventQueueProvider> logger
    )
    {
        this.logger = logger;
        _cachedProcessingSequence = [.. queuesListInDefaultProcessingSequence.Select(k => serviceProvider.GetRequiredKeyedService<IEventQueue>(k))];
    }

    public IEventQueue[] DefaultProcessingSequence => _cachedProcessingSequence;

    public IEnumerable<IEventQueue> AllQueues => _cachedProcessingSequence;

    public IEventQueue GetQueue(string name)
    {
        var q = _cachedProcessingSequence.SingleOrDefault(x => x.Name.Equals(name))
            ?? throw new KeyNotFoundException($"No event queue with name '{name}' is registered. Available queues: {string.Join(", ", _cachedProcessingSequence.Select(x => x.Name))}");
        return q;
    }
}
