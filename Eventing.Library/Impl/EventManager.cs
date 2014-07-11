using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Eventing.Library.Impl {
    /// <summary>
    ///     Implementation of IEventManager. Thread safe when used message bus is thread safe.
    /// </summary>
    public class EventManager : IEventManager {
        private readonly IMessageBus messageBus;

        private readonly TraceSource trace = new TraceSource("Eventing.Library.EventManager");

        public EventManager(IMessageBus messageBus) {
            this.messageBus = messageBus;
        }

        /// <summary>
        ///     Raises the <paramref name="event" />
        /// </summary>
        /// <param name="event">The event to raise</param>
        public void RaiseEvent(IEvent @event) {
            this.trace.TraceEvent(TraceEventType.Information, 0, "Event: {0}", @event);

            this.messageBus.Send(@event);
        }

        /// <summary>
        ///     Creates a task which waits for an event. This event should match the <paramref name="filter" />
        ///     and should be an instance of expected type from <paramref name="eventTypes" /> to terminate the task.
        /// </summary>
        /// <param name="timeout">Timout after which task will be terminated with null result</param>
        /// <param name="filter">The filter for events - a function that returns true if event matches</param>
        /// <param name="eventTypes">Expected event types</param>
        /// <returns>
        ///     Created task. Task result is event of expected type from <paramref name="eventTypes" /> or null if task timed out.
        /// </returns>
        public Task<IEvent> WaitFor(TimeSpan? timeout, Func<IEvent, bool> filter = null, params Type[] eventTypes) {
            if (eventTypes.Length == 0)
                throw new ArgumentException("event types can not be empty", "eventTypes");

            var taskCompletionSource = new TaskCompletionSource<IEvent>();

            var subscriptionId = Guid.NewGuid();

            var subscription = new MessageSubscription(
                subscriptionId,
                message => {
                    var @event = message as IEvent;
                    if (filter != null && !filter(@event))
                        return;

                    if (taskCompletionSource.TrySetResult(@event))
                        this.trace.TraceEvent(TraceEventType.Information, 0, "Wait ended: '{0}' - '{1}'",
                            subscriptionId, message.GetType());
                },
                this,
                UnsubscribePolicy.Auto, null,
                eventTypes);

            this.messageBus.Subscribe(subscription);

            if (timeout.HasValue) {
                var timer = new Timer(e => {
                    if (taskCompletionSource.TrySetResult(null)) {
                        this.messageBus.Unsubscribe(subscription);
                        this.trace.TraceEvent(TraceEventType.Information, 0, "Wait ended: '{0}' - Timeout",
                            subscriptionId);
                    }
                });
                timer.Change(timeout.Value, TimeSpan.Zero);
            }

            if (this.trace.Switch.ShouldTrace(TraceEventType.Information))
                this.trace.TraceEvent(TraceEventType.Information, 0, "Waiting: '{0}' ({1})({2})",
                    string.Join(",", eventTypes.AsEnumerable()), subscriptionId,
                    timeout.HasValue ? timeout.Value : TimeSpan.Zero);

            return taskCompletionSource.Task;
        }

        /// <summary>
        ///     Stop receiving all events
        /// </summary>
        /// <param name="listener">Event listener</param>
        public void StopReceiving(object listener) {
            this.trace.TraceEvent(TraceEventType.Information, 0, "Stop receiving: '{0}' - all events",
                listener.GetType());

            this.messageBus.Unsubscribe(listener);
        }

        /// <summary>
        ///     Stop receiving events of type <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">Expected event type</typeparam>
        /// <param name="listener">Event listener</param>
        public void StopReceiving<T>(object listener) {
            this.trace.TraceEvent(TraceEventType.Information, 0, "Stop receiving: '{0}' - {1}", listener.GetType(),
                typeof (T));

            this.messageBus.Unsubscribe(listener, typeof (T));
        }

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
        public void StartReceiving<T>(Action<T> handler, Func<T, bool> filter = null,
            SynchronizationContext context = null) where T : IEvent {
            if (handler.Target == null)
                throw new ArgumentException("Handler target unspecified. Please provide listener manually.", "handler");

            this.StartReceiving(handler, handler.Target, filter, context);
        }

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
        public void StartReceiving<T>(Action<T> handler, object listener, Func<T, bool> filter = null,
            SynchronizationContext context = null) where T : IEvent {
            var subscription = new MessageSubscription(
                message => {
                    if (filter == null || filter((T) message))
                        handler((T) message);
                },
                listener,
                UnsubscribePolicy.Manual, context,
                typeof (T));

            this.trace.TraceEvent(TraceEventType.Information, 0, "Receiving: '{0}' - '{1}'",
                listener.GetType(), typeof (T));

            this.messageBus.Subscribe(subscription);
        }

        #region WaitFor helpers

        /// <summary>
        ///     Creates a task which waits for an event. This event should match the <paramref name="filter" />
        ///     and should be an instance of expected type to terminate the task.
        /// </summary>
        /// <typeparam name="T">Expected event type</typeparam>
        /// <param name="timeout">Timout after which task terminates with null result</param>
        /// <param name="filter">The filter for events - a function that returns true if event matches</param>
        /// <returns>Created task. Result of this task is event of expected type or null</returns>
        public Task<IEvent> WaitFor<T>(TimeSpan? timeout = null, Func<IEvent, bool> filter = null)
            where T : IEvent {
            return this.WaitFor(timeout, filter, typeof (T));
        }

        /// <summary>
        ///     Creates a task which waits for an event. This event should match the <paramref name="filter" />
        ///     and should be an instance of expected type to terminate the task.
        /// </summary>
        /// <typeparam name="T1">Expected event type 1</typeparam>
        /// <typeparam name="T2">Expected event type 2</typeparam>
        /// <param name="timeout">Timout after which task terminates with null result</param>
        /// <param name="filter">The filter for events - a function that returns true if event matches</param>
        /// <returns>Created task. Result of this task is event of expected type or null</returns>
        public Task<IEvent> WaitFor<T1, T2>(TimeSpan? timeout = null, Func<IEvent, bool> filter = null)
            where T1 : IEvent
            where T2 : IEvent {
            return this.WaitFor(timeout, filter, typeof (T1), typeof (T2));
        }

        /// <summary>
        ///     Creates a task which waits for an event. This event should match the <paramref name="filter" />
        ///     and should be an instance of expected type to terminate the task.
        /// </summary>
        /// <typeparam name="T1">Expected event type 1</typeparam>
        /// <typeparam name="T2">Expected event type 2</typeparam>
        /// <typeparam name="T3">Expected event type 3</typeparam>
        /// <param name="timeout">Timout after which task terminates with null result</param>
        /// <param name="filter">The filter for events - a function that returns true if event matches</param>
        /// <returns>Created task. Result of this task is event of expected type or null</returns>
        public Task<IEvent> WaitFor<T1, T2, T3>(TimeSpan? timeout = null, Func<IEvent, bool> filter = null)
            where T1 : IEvent
            where T2 : IEvent
            where T3 : IEvent {
            return this.WaitFor(timeout, filter, typeof (T1), typeof (T2), typeof (T3));
        }

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
        /// <returns>Created task. Result of this task is event of expected type or null</returns>
        public Task<IEvent> WaitFor<T1, T2, T3, T4>(TimeSpan? timeout = null, Func<IEvent, bool> filter = null)
            where T1 : IEvent
            where T2 : IEvent
            where T3 : IEvent
            where T4 : IEvent {
            return this.WaitFor(timeout, filter, typeof (T1), typeof (T2), typeof (T3), typeof (T4));
        }

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
        /// <returns>Created task. Result of this task is event of expected type or null</returns>
        public Task<IEvent> WaitFor<T1, T2, T3, T4, T5>(TimeSpan? timeout = null, Func<IEvent, bool> filter = null)
            where T1 : IEvent
            where T2 : IEvent
            where T3 : IEvent
            where T4 : IEvent
            where T5 : IEvent {
            return this.WaitFor(timeout, filter, typeof (T1), typeof (T2), typeof (T3), typeof (T4), typeof (T5));
        }

        #endregion
    }
}