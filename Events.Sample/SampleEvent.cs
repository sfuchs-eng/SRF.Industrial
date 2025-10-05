using System;
using SRF.Industrial.Events.Abstractions;

namespace SRF.Industrial.Events.Sample;

/// <summary>
/// An event as it could be received from an event source, e.g. via MQTT, WebSocket, OPC UA, ...
/// </summary>
public class SampleEvent : IEvent
{
    public Guid Id { get; init; }
    public DateTime CreatedAt { get; init; }
    public required string Message { get; set; }
    public int? DerivedProperty { get; set; }
}
