namespace SRF.Industrial.Events.Abstractions;

public interface IEventQueueProvider
{
    public IEventQueue GetQueue(string name);
    public IEventQueue[] DefaultProcessingSequence { get; }
    public IEnumerable<IEventQueue> AllQueues { get; }
}
