using System;
using System.Collections.Generic;

namespace Eventing.Library {
    public interface IMessageSubscription {
        /// <summary>
        ///     Subscription's identifier
        /// </summary>
        Guid Id { get; }

        /// <summary>
        ///     Subscription's owner
        /// </summary>
        object Owner { get; }

        /// <summary>
        ///     Unsubscribe policy
        /// </summary>
        UnsubscribePolicy UnsubscribePolicy { get; }

        /// <summary>
        ///     Expected types
        /// </summary>
        IReadOnlyCollection<Type> MessagesTypes { get; }

        /// <summary>
        ///     Determs whether the subscription accepts events of type <paramref name="messageType" />.
        /// </summary>
        /// <param name="messageType">Type to check</param>
        /// <returns>True if subscription accepts messages of type <paramref name="messageType" />, otherwise - false</returns>
        bool Contains(Type messageType);

        /// <summary>
        ///     Processes the <paramref name="message" />.
        /// </summary>
        /// <param name="message">The message to proccess</param>
        void ProccessMessage(object message);
    }
}