using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Eventing.Library.Impl {
    public class MessageSubscription : IMessageSubscription {
        /// <summary>
        ///     Subscription's handler
        /// </summary>
        private readonly Action<object> handler;

        /// <summary>
        ///     Expected types
        /// </summary>
        private readonly Type[] messagesTypes;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="id">Subscription's identifier</param>
        /// <param name="handler">Subscription's handler</param>
        /// <param name="owner">Subscription's owner</param>
        /// <param name="unsubscribePolicy">Unsubscribe policy</param>
        /// <param name="synchronizationContext">
        ///     Context in which <paramref name="handler" /> should be executed. If null
        ///     SynchronizationContext.Current is used.
        /// </param>
        /// <param name="messagesTypes">Expected message types</param>
        public MessageSubscription(Guid id, Action<object> handler, object owner, UnsubscribePolicy unsubscribePolicy,
            SynchronizationContext synchronizationContext,
            params Type[] messagesTypes) {
            this.messagesTypes = messagesTypes;
            this.handler = handler;
            this.Owner = owner;
            this.Id = id;
            this.UnsubscribePolicy = unsubscribePolicy;
            this.SynchronizationContext = synchronizationContext ?? SynchronizationContext.Current;

            if (this.SynchronizationContext == null)
                throw new ArgumentNullException("synchronizationContext",
                    "synchronizationContext or SynchronizationContext.Current must be not null");
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="handler"> Subscription's handler</param>
        /// <param name="owner">Subscription's owner</param>
        /// <param name="unsubscribePolicy">Unsubscribe policy</param>
        /// <param name="synchronizationContext">
        ///     Context in which <paramref name="handler" /> should be executed. If null
        ///     SynchronizationContext.Current is used.
        /// </param>
        /// <param name="messagesTypes">Expected message types</param>
        public MessageSubscription(Action<object> handler, object owner, UnsubscribePolicy unsubscribePolicy,
            SynchronizationContext synchronizationContext,
            params Type[] messagesTypes)
            : this(Guid.NewGuid(), handler, owner, unsubscribePolicy, synchronizationContext, messagesTypes) {
        }

        /// <summary>
        ///     Context in which handler should be executed
        /// </summary>
        public SynchronizationContext SynchronizationContext { get; private set; }

        /// <summary>
        ///     Subscription's identifier
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        ///     Subscription's owner
        /// </summary>
        public object Owner { get; private set; }

        /// <summary>
        ///     Unsubscribe policy
        /// </summary>
        public UnsubscribePolicy UnsubscribePolicy { get; set; }

        /// <summary>
        ///     Expected types
        /// </summary>
        public IReadOnlyCollection<Type> MessagesTypes {
            get { return this.messagesTypes; }
        }

        /// <summary>
        ///     Determs whether the subscription accepts events of type <paramref name="messageType" />.
        /// </summary>
        /// <param name="messageType">Type to check</param>
        /// <returns>True if subscription accepts messages of type <paramref name="messageType" />, otherwise - false</returns>
        public bool Contains(Type messageType) {
            return this.messagesTypes.Any(type => messageType == type || type.IsAssignableFrom(messageType));
        }

        /// <summary>
        ///     Processes the <paramref name="message" />.
        /// </summary>
        /// <param name="message">The message to proccess</param>
        public void ProccessMessage(object message) {
            var messageHandler = this.handler;
            this.SynchronizationContext.Post(o => messageHandler(message), null);
        }

        /// <summary>
        ///     Creates message subscription
        /// </summary>
        /// <typeparam name="T">Expected event type</typeparam>
        /// <param name="handler">Subscription's handler</param>
        /// <param name="owner">Subscription's owner</param>
        /// <param name="unsubscribePolicy">Unsubscribe policy</param>
        /// <param name="synchronizationContext">
        ///     Context in which <paramref name="handler" /> should be executed. If null
        ///     SynchronizationContext.Current is used.
        /// </param>
        /// <returns>Created message subscription</returns>
        public static MessageSubscription Create<T>(Action<object> handler, object owner = null,
            UnsubscribePolicy unsubscribePolicy = UnsubscribePolicy.Manual,
            SynchronizationContext synchronizationContext = null) {
            return new MessageSubscription(handler, owner, unsubscribePolicy, synchronizationContext, typeof (T));
        }

        /// <summary>
        ///     Creates message subscription
        /// </summary>
        /// <typeparam name="T1">Expected event type 1</typeparam>
        /// <typeparam name="T2">Expected event type 2</typeparam>
        /// <param name="handler">Subscription's handler</param>
        /// <param name="owner">Subscription's owner</param>
        /// <param name="unsubscribePolicy">Unsubscribe policy</param>
        /// <param name="synchronizationContext">
        ///     Context in which <paramref name="handler" /> should be executed. If null
        ///     SynchronizationContext.Current is used.
        /// </param>
        /// <returns>Created message subscription</returns>
        public static MessageSubscription Create<T1, T2>(Action<object> handler, object owner = null,
            UnsubscribePolicy unsubscribePolicy = UnsubscribePolicy.Manual,
            SynchronizationContext synchronizationContext = null) {
            return new MessageSubscription(handler, owner, unsubscribePolicy, synchronizationContext, typeof (T1),
                typeof (T2));
        }

        /// <summary>
        ///     Creates message subscription
        /// </summary>
        /// <typeparam name="T1">Expected event type 1</typeparam>
        /// <typeparam name="T2">Expected event type 2</typeparam>
        /// <typeparam name="T3">Expected event type 3</typeparam>
        /// <param name="handler">Subscription's handler</param>
        /// <param name="owner">Subscription's owner</param>
        /// <param name="unsubscribePolicy">Unsubscribe policy</param>
        /// <param name="synchronizationContext">
        ///     Context in which <paramref name="handler" /> should be executed. If null
        ///     SynchronizationContext.Current is used.
        /// </param>
        /// <returns>Created message subscription</returns>
        public static MessageSubscription Create<T1, T2, T3>(Action<object> handler, object owner = null,
            UnsubscribePolicy unsubscribePolicy = UnsubscribePolicy.Manual,
            SynchronizationContext synchronizationContext = null) {
            return new MessageSubscription(handler, owner, unsubscribePolicy, synchronizationContext, typeof (T1),
                typeof (T2), typeof (T3));
        }
    }
}