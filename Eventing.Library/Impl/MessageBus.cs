using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Eventing.Library.Impl {
    /// <summary>
    ///     Thread safe implementation of IMessageBus
    /// </summary>
    public class MessageBus : IMessageBus {
        private readonly List<IMessageSubscription> subscriptions;

        private readonly TraceSource trace = new TraceSource("Eventing.Library.MessageBus");

        public MessageBus() {
            this.subscriptions = new List<IMessageSubscription>();
        }

        /// <summary>
        ///     Gets subscriptions
        /// </summary>
        /// <returns>Active subscriptions</returns>
        public IReadOnlyCollection<IMessageSubscription> GetSubscriptions() {
            lock (this.subscriptions)
                return this.subscriptions.ToArray();
        }

        /// <summary>
        ///     Sends the <paramref name="message" /> to the bus.
        /// </summary>
        /// <param name="message">The message to send</param>
        public void Send(object message) {
            var messageType = message.GetType();
            IMessageSubscription[] subscriptionsForMessage;

            lock (this.subscriptions) {
                subscriptionsForMessage = this.subscriptions
                    .Where(s => s.MessagesTypes.Any(type => messageType == type || type.IsAssignableFrom(messageType)))
                    .ToArray();
            }

            this.trace.TraceEvent(TraceEventType.Information, 0, "Processing message {0}. ({1} subscription[s]).",
                messageType, subscriptionsForMessage.Length);

            if (this.trace.Switch.ShouldTrace(TraceEventType.Verbose))
                this.trace.TraceEvent(TraceEventType.Verbose, 0, "Subscriptions:\n{0}",
                    string.Join(" ", subscriptionsForMessage.Select(x => x.Id)));

            foreach (var subscription in subscriptionsForMessage)
                subscription.ProccessMessage(message);

            this.UnsubscribeAutoSubscriptions(subscriptionsForMessage);

            this.trace.TraceEvent(TraceEventType.Verbose, 0, "Message sended: '{0}'", message.GetType());
        }

        /// <summary>
        ///     Adds <paramref name="messageSubscription" /> to the bus.
        /// </summary>
        /// <param name="messageSubscription">Message subscriptions</param>
        public void Subscribe(IMessageSubscription messageSubscription) {
            lock (this.subscriptions)
                this.subscriptions.Add(messageSubscription);

            this.trace.TraceEvent(TraceEventType.Verbose, 0, "Subscription added: '{0}'", messageSubscription.Id);
        }

        /// <summary>
        ///     Removes the <paramref name="messageSubscription" /> from the bus.
        /// </summary>
        /// <param name="messageSubscription">Message subscription</param>
        public void Unsubscribe(IMessageSubscription messageSubscription) {
            lock (this.subscriptions)
                this.subscriptions.Remove(messageSubscription);

            this.trace.TraceEvent(TraceEventType.Verbose, 0, "Subscription removed: '{0}'", messageSubscription.Id);
        }

        /// <summary>
        ///     Removes <paramref name="messageSubscriptions" /> from the bus.
        /// </summary>
        /// <param name="messageSubscriptions">Message subscriptions</param>
        public void Unsubscribe(IReadOnlyCollection<IMessageSubscription> messageSubscriptions) {
            lock (this.subscriptions)
                this.subscriptions.RemoveAll(messageSubscriptions.Contains);

            if (this.trace.Switch.ShouldTrace(TraceEventType.Verbose))
                this.trace.TraceEvent(TraceEventType.Verbose, 0, "Subscriptions removed: '{0}'",
                    string.Join(" ", messageSubscriptions.Select(x => x.Id)));
        }

        /// <summary>
        ///     Removes all subsciptions owned by the <paramref name="owner" />.
        /// </summary>
        /// <param name="owner">Subscriptions owner</param>
        public void Unsubscribe(object owner) {
            lock (this.subscriptions)
                this.subscriptions.RemoveAll(handler => handler.Owner == owner);
        }

        /// <summary>
        ///     Removes all subsciptions owned by the <paramref name="owner" /> those contains <paramref name="type" />.
        /// </summary>
        /// <param name="owner">Subscriptions owner</param>
        /// <param name="type"></param>
        public void Unsubscribe(object owner, Type type) {
            lock (this.subscriptions)
                this.subscriptions.RemoveAll(handler => handler.Owner == owner && handler.Contains(type));
        }

        /// <summary>
        ///     Removes subscriptions with auto unsubscribe policy.
        /// </summary>
        /// <param name="subscriptionsForMessage"></param>
        private void UnsubscribeAutoSubscriptions(IEnumerable<IMessageSubscription> subscriptionsForMessage) {
            var subscriptionsToRemove = subscriptionsForMessage
                .Where(s => s.UnsubscribePolicy == UnsubscribePolicy.Auto)
                .ToArray();

            if (!subscriptionsToRemove.Any())
                return;

            lock (this.subscriptions) {
                this.subscriptions.RemoveAll(subscriptionsToRemove.Contains);
            }
        }
    }
}