using System;
using System.Collections.Generic;

namespace Eventing.Library {
    public interface IMessageBus {
        /// <summary>
        ///     Gets subscriptions
        /// </summary>
        /// <returns>Active subscriptions</returns>
        IReadOnlyCollection<IMessageSubscription> GetSubscriptions();

        /// <summary>
        ///     Sends the <paramref name="message" /> to the bus.
        /// </summary>
        /// <param name="message">The message to send</param>
        void Send(object message);

        /// <summary>
        ///     Adds <paramref name="messageSubscription" /> to the bus.
        /// </summary>
        /// <param name="messageSubscription">Message subscriptions</param>
        void Subscribe(IMessageSubscription messageSubscription);

        /// <summary>
        ///     Removes the <paramref name="messageSubscription" /> from the bus.
        /// </summary>
        /// <param name="messageSubscription">Message subscription</param>
        void Unsubscribe(IMessageSubscription messageSubscription);

        /// <summary>
        ///     Removes <paramref name="messageSubscriptions" /> from the bus.
        /// </summary>
        /// <param name="messageSubscriptions">Message subscriptions</param>
        void Unsubscribe(IReadOnlyCollection<IMessageSubscription> messageSubscriptions);

        /// <summary>
        ///     Removes all subsciptions owned by the <paramref name="owner" />.
        /// </summary>
        /// <param name="owner">Subscriptions owner</param>
        void Unsubscribe(object owner);

        /// <summary>
        ///     Removes all subsciptions owned by the <paramref name="owner" /> those contains <paramref name="type" />.
        /// </summary>
        /// <param name="owner">Subscriptions owner</param>
        /// <param name="type"></param>
        void Unsubscribe(object owner, Type type);
    }
}