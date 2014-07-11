using System;
using System.Threading;
using System.Threading.Tasks;

namespace Eventing.Library {
    public interface IEventManager {
        /// <summary>
        ///     Raises the <paramref name="event" />
        /// </summary>
        /// <param name="event">The event to raise</param>
        void RaiseEvent(IEvent @event);

        /// <summary>
        ///     Creates a task which waits for an event. This event should match the <paramref name="filter" />
        ///     and should be an instance of expected type from <paramref name="eventTypes" /> to finish the task.
        /// </summary>
        /// <param name="timeout">Timout after which task will be terminated with null result</param>
        /// <param name="filter">The filter for events - a function that returns true if event matches</param>
        /// <param name="eventTypes">Expected event types</param>
        /// <returns>
        ///     Created task. Task result is event of expected type from <paramref name="eventTypes" /> or null if task timed out.
        /// </returns>
        Task<IEvent> WaitFor(TimeSpan? timeout, Func<IEvent, bool> filter, params Type[] eventTypes);

        /// <summary>
        ///     Creates a task which waits for an event. This event should match the <paramref name="filter" />
        ///     and should be an instance of expected type to terminate the task.
        /// </summary>
        /// <typeparam name="T">Expected event type</typeparam>
        /// <param name="timeout">Timout after which task terminates with null result</param>
        /// <param name="filter">The filter for events - a function that returns true if event matches</param>
        /// <returns>Created task. Result of this task is event of expected type or null.</returns>
        Task<IEvent> WaitFor<T>(TimeSpan? timeout = null, Func<IEvent, bool> filter = null) where T : IEvent;

        /// <summary>
        ///     Creates a task which waits for an event. This event should match the <paramref name="filter" />
        ///     and should be an instance of expected type to terminate the task.
        /// </summary>
        /// <typeparam name="T1">Expected event type 1</typeparam>
        /// <typeparam name="T2">Expected event type 2</typeparam>
        /// <param name="timeout">Timout after which task terminates with null result</param>
        /// <param name="filter">The filter for events - a function that returns true if event matches</param>
        /// <returns>Created task. Result of this task is event of expected type or null.</returns>
        Task<IEvent> WaitFor<T1, T2>(TimeSpan? timeout = null, Func<IEvent, bool> filter = null)
            where T1 : IEvent
            where T2 : IEvent;

        /// <summary>
        ///     Creates a task which waits for an event. This event should match the <paramref name="filter" />
        ///     and should be an instance of expected type to terminate the task.
        /// </summary>
        /// <typeparam name="T1">Expected event type 1</typeparam>
        /// <typeparam name="T2">Expected event type 2</typeparam>
        /// <typeparam name="T3">Expected event type 3</typeparam>
        /// <param name="timeout">Timout after which task terminates with null result</param>
        /// <param name="filter">The filter for events - a function that returns true if event matches</param>
        /// <returns>Created task. Result of this task is event of expected type or null.</returns>
        Task<IEvent> WaitFor<T1, T2, T3>(TimeSpan? timeout = null, Func<IEvent, bool> filter = null)
            where T1 : IEvent
            where T2 : IEvent
            where T3 : IEvent;

        /// <summary>
        ///     Creates a task which waits for an event. This event should match the <paramref name="filter" />
        ///     and should be an instance of expected type to terminate the task.
        /// </summary>
        /// <typeparam name="T1">Expected event type 1</typeparam>
        /// <typeparam name="T2">Expected event type 2</typeparam>
        /// <typeparam name="T3">Expected event type 3</typeparam>
        /// <typeparam name="T4">Expected event type 4</typeparam>
        /// <param name="timeout">Timout after which task terminates with null result</param>
        /// <param name="filter">The filter for events - a function that returns true if event matches</param>
        /// <returns>Created task. Result of this task is event of expected type or null.</returns>
        Task<IEvent> WaitFor<T1, T2, T3, T4>(TimeSpan? timeout = null, Func<IEvent, bool> filter = null)
            where T1 : IEvent
            where T2 : IEvent
            where T3 : IEvent
            where T4 : IEvent;

        /// <summary>
        ///     Creates a task which waits for an event. This event should match the <paramref name="filter" />
        ///     and should be an instance of expected type to terminate the task.
        /// </summary>
        /// <typeparam name="T1">Expected event type 1</typeparam>
        /// <typeparam name="T2">Expected event type 2</typeparam>
        /// <typeparam name="T3">Expected event type 3</typeparam>
        /// <typeparam name="T4">Expected event type 4</typeparam>
        /// <typeparam name="T5">Expected event type 5</typeparam>
        /// <param name="timeout">Timout after which task terminates with null result</param>
        /// <param name="filter">The filter for events - a function that returns true if event matches</param>
        /// <returns>Created task. Result of this task is event of expected type or null.</returns>
        Task<IEvent> WaitFor<T1, T2, T3, T4, T5>(TimeSpan? timeout = null, Func<IEvent, bool> filter = null)
            where T1 : IEvent
            where T2 : IEvent
            where T3 : IEvent
            where T4 : IEvent
            where T5 : IEvent;

        /// <summary>
        ///     Starts receiving events of type <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">Expected event type</typeparam>
        /// <param name="handler">Event handler</param>
        /// <param name="filter">The filter for events - a function that returns true if event matches</param>
        /// <param name="context">
        ///     Context in which <paramref name="handler" /> should be executed. If null
        ///     SynchronizationContext.Current is used.
        /// </param>
        void StartReceiving<T>(Action<T> handler, Func<T, bool> filter = null, SynchronizationContext context = null)
            where T : IEvent;

        /// <summary>
        ///     Starts receiving events of type <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">Expected event type</typeparam>
        /// <param name="handler">Event handler</param>
        /// <param name="listener">Event listener</param>
        /// <param name="filter">The filter for events - a function that returns true if event matches</param>
        /// <param name="context">
        ///     Context in which <paramref name="handler" /> should be executed. If null
        ///     SynchronizationContext.Current is used.
        /// </param>
        void StartReceiving<T>(Action<T> handler, object listener, Func<T, bool> filter = null,
            SynchronizationContext context = null) where T : IEvent;

        /// <summary>
        ///     Stop receiving all events.
        /// </summary>
        /// <param name="listener">Event listener</param>
        void StopReceiving(object listener);

        /// <summary>
        ///     Stop receiving events of type <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">Expected event type</typeparam>
        /// <param name="listener">Event listener</param>
        void StopReceiving<T>(object listener);
    }
}