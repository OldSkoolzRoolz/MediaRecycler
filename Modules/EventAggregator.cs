// Project Name: ${File.ProjectName}
// Author:  Kyle Crowder 
// Github:  OldSkoolzRoolz
// Distributed under Open Source License 
// Do not remove file headers




using System.Collections.Concurrent;



namespace MediaRecycler.Modules;


public interface IMessage
{

}


public interface IEventAggregator
{

    void Publish<T>(T message) where T : IMessage;

    void Subscribe<T>(Action<T> handler) where T : IMessage;

    void Unsubscribe<T>(Action<T> handler) where T : IMessage;

}


/// <summary>
///     Provides a mechanism for managing event-based communication between components.
///     This class implements the publish-subscribe pattern, allowing components to
///     subscribe to specific event types and receive notifications when those events are published.
///     This allows for decoupled communication between components, enhancing modularity and testability.
///     Each handler is unique to the specific type of event it subscribes to, and can be handled differently allowing
///     for a more granular control over event handling. Each subscriber can register a handler for a specific type of
///     event,
///     and can process each event independently.
/// </summary>
public class EventAggregator : IEventAggregator
{

    private readonly ConcurrentDictionary<Type, List<Delegate>> _subscribers = new();







    /// <summary>
    ///     Publishes a message of the specified type to all subscribed handlers.
    ///     This method allows components to broadcast events or data to other components
    ///     that have subscribed to the specified type of message.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the message being published. This type is used to match
    ///     the message with the appropriate subscribers.
    /// </typeparam>
    /// <param name="message">
    ///     The message instance to be published. This message is passed to all
    ///     subscribed handlers of the specified type.
    /// </param>
    public void Publish<T>(T message) where T : IMessage
    {
        if (_subscribers.TryGetValue(typeof(T), out var handlers))
        {
            List<Delegate> snapshot;

            lock (handlers)
            {
                snapshot = [.. handlers];
            }

            foreach (var handler in snapshot)
            {
                ((Action<T>)handler)?.Invoke(message);
            }
        }
    }







    /// <summary>
    ///     Subscribes to events of the specified type.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of event to subscribe to. This type represents the message or event data.
    /// </typeparam>
    /// <param name="handler">
    ///     The action to invoke when an event of type <typeparamref name="T" /> is published.
    /// </param>
    /// <remarks>
    ///     The provided <paramref name="handler" /> will be called whenever an event of type <typeparamref name="T" />
    ///     is published through the <see cref="EventAggregator" />. Ensure that the handler is thread-safe as it may
    ///     be invoked from different threads.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if the <paramref name="handler" /> is <c>null</c>.
    /// </exception>
    public void Subscribe<T>(Action<T> handler) where T : IMessage
    {
        var handlers = _subscribers.GetOrAdd(typeof(T), _ => []);

        lock (handlers)
        {
            handlers.Add(handler);
        }
    }







    /// <summary>
    ///     Unsubscribes from events of the specified type.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of event to unsubscribe from. This type represents the message or event data.
    /// </typeparam>
    /// <param name="handler">
    ///     The action that was previously subscribed to events of type <typeparamref name="T" />.
    /// </param>
    /// <remarks>
    ///     The provided <paramref name="handler" /> will no longer be invoked when an event of type
    ///     <typeparamref name="T" /> is published. Ensure that the handler matches the one used during subscription.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if the <paramref name="handler" /> is <c>null</c>.
    /// </exception>
    public void Unsubscribe<T>(Action<T> handler) where T : IMessage
    {
        if (_subscribers.TryGetValue(typeof(T), out var handlers))
        {
            lock (handlers)
            {
                _ = handlers.Remove(handler);
            }
        }
    }

}


public class StatusMessage(
            string text) : IMessage
{

    public string Text { get; } = text;

}


public class PageNumberMessage(
            int pageNumber) : IMessage
{

    public int PageNumber { get; } = pageNumber;

}


public class QueueCountMessage(
            int queueCount) : IMessage
{

    public int QueueCount { get; } = queueCount;

}


public class PuppeteerRecoveryEvent(
            string message) : IMessage
{

    public string message { get; } = message;

}

public class StatusBarMessage(
            string text) : IMessage
{
    public string Text { get; } = text;
}