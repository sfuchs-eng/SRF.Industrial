# SRF.Industrial.Events

## Purpose

A framework foreseen to receive, transform and handle events from (physical) systems, received through arbitrary connections or internally generated.
Events can be routed across an arbitrary sequence of processing queues. The predefined default is first a transformation and thereafter processing (the actual handling) via another queue.

Originally created to handle events in a home automation context, it can as well serve the handling of factory machine events and status transitions.

## Function principle

### Overview key types

![Key types overview](Overview.svg)

### Base structure

Application specific implementations of `EventReceiver : IEventReceiver` and `IEventContextFactory<TEvent>` would serve the creation of `IEvent` objects encapsulated in a metadata container `IEventContext<TEvent>`. The receiver must, after creation of the context, pass the `IEventContext` object to
it's first queue. Once all handlers of a queue are processed, the `IEventHandlingDispatcher` processing the queue items hands the event context
onwards to the next queue as per the list in the context object.

The framework uses dedicated queues, keyed singletons in DI, and background workers `BackgroundService` to execute the registered event handlers.
While the `EventTransformationDispatcher` ensures sequential execution of the registered handlers, the subsequent `EventProcessingDispatcher` calls
all registered event handlers at once via the thread pool.

If `IServiceCollection AddBasicEventsProcessing<TEvent>(this IServiceCollection services) where TEvent : class, IEvent` from the namespace
`SRF.Industrial.Events.Abstractions` is used to register all related services, the transformation and processing queues are available as DI services:

- `IServiceProvider.GetRequiredKeyedService<IEventQueue>("EventTransformation")`

- `IServiceProvider.GetRequiredKeyedService<IEventQueue>("EventProcessing")`

### Application specific

Instanciate as many `IEventReceiver`s as required, preferably by inheriting from the abstract class `EventReceiver`.
Ensure their registration as hosted services via `services.AddHostedService<..>()`.

Provide one or more custom `IEvent` per receiver and one signle corresponding `IEventContext<TEvent>` implementation per receiver.
For simple scenarios the implementation of the `IEvent` might be sufficient, covering `IEventContext<TEvent>` with the generic implementation
provided by the library: `EventContext<TEvent>`

