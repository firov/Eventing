using System;
using System.Threading;
using Eventing.Library;
using Eventing.Library.Impl;
using NUnit.Framework;

namespace Eventing.Tests {
    public abstract class TestBase {
        private int reactIndex = 1;

        private SingleThreadSynchronizationContext synchronizationContext;

        protected IMessageBus MessageBus { get; private set; }
        protected IEventManager EventManager { get; private set; }

        [TearDown]
        public virtual void Cleanup() {
            this.synchronizationContext.Complete();
        }

        [SetUp]
        public virtual void Initialize() {
            this.synchronizationContext = new SingleThreadSynchronizationContext();

            SynchronizationContext.SetSynchronizationContext(this.synchronizationContext);

            this.MessageBus = new MessageBus();
            this.EventManager = new EventManager(this.MessageBus);
        }

        protected void React() {
            Console.WriteLine("React #{0}", this.reactIndex++);

            while (this.synchronizationContext.RunAllOperations())
                ;
        }
    }
}