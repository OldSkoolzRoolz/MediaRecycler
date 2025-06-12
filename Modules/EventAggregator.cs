// "Open Source copyrights apply - All code can be reused DO NOT remove author tags"




using System.Collections.Concurrent;



namespace MediaRecycler.Modules;


public interface IEventAggregator
{

    void Subscribe<T>(Action<T> handler);

    void Unsubscribe<T>(Action<T> handler);

    void Publish<T>(T message);

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
    public void Subscribe<T>(Action<T> handler)
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
    public void Unsubscribe<T>(Action<T> handler)
    {
        if (_subscribers.TryGetValue(typeof(T), out var handlers))
        {
            lock (handlers)
            {
                _ = handlers.Remove(handler);
            }
        }
    }






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
    public void Publish<T>(T message)
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

}


public class StatusMessage(
            string text)
{

    public string Text { get; } = text;

}


public class PageNumberMessage(
            int pageNumber)
{

    public int PageNumber { get; } = pageNumber;

}
