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
        object[] defaultProcessingSequence,
        ILogger<EventQueueProvider> logger
    )
    {
        this.logger = logger;
        _cachedProcessingSequence = [.. defaultProcessingSequence.Select(k => serviceProvider.GetRequiredKeyedService<IEventQueue>(k))];
    }

    public IEventQueue[] DefaultProcessingSequence => _cachedProcessingSequence;

    public IEventQueue GetQueue(string name)
    {
        return _cachedProcessingSequence.Single(x => x.Name.Equals(name));
    }
}
