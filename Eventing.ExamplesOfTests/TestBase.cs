using System.Threading;
using Eventing.Library;
using Eventing.Library.Impl;
using NLog;
using NLog.Config;
using NUnit.Framework;

namespace Eventing.ExamplesOfTests {
    public abstract class TestBase {
        private int reactIndex;
        private SingleThreadSynchronizationContext synchronizationContext;

        protected TestBase() {
        }

        protected IEventManager EventManager { get; private set; }

        protected Logger Log { get; private set; }

        [TearDown]
        public virtual void Cleanup() {
            this.EventManager.StopReceiving(this);

            this.synchronizationContext.Complete();
        }

        [SetUp]
        public virtual void Initialize() {
            LogManager.Configuration = XmlLoggingConfiguration.AppConfig;

            this.synchronizationContext = new SingleThreadSynchronizationContext();
            this.Log = LogManager.GetLogger("Tests");

            SynchronizationContext.SetSynchronizationContext(this.synchronizationContext);

            this.EventManager = new EventManager(new MessageBus());
        }

        protected void React() {
            this.Log.Warn("React #{0}", this.reactIndex++);

            while (this.synchronizationContext.RunAllOperations())
                ;
        }
    }
}