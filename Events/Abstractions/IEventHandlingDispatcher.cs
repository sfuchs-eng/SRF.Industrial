using Microsoft.Extensions.Hosting;

namespace SRF.Industrial.Events.Abstractions;

/// <summary>
/// A hosted service that dispatches events from an event queue to registered handlers
/// </summary>
public interface IEventHandlingDispatcher : IHostedService
{
    /// <summary>
    /// For identification/logging purposes
    /// </summary>
    string Name { get; }
}
