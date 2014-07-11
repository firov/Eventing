using System;
using System.Threading;
using System.Threading.Tasks;
using Eventing.Examples.Events;
using Eventing.Library;
using Eventing.Library.Impl;
using NLog;

namespace Eventing.Examples.Impl {
    /// <summary>
    ///     All execution will be performed in "Main thread"
    /// </summary>
    internal class SingleThreadUsage : IExample {
        private static readonly Logger Log = LogManager.GetLogger("Eventing.Examples");

        private readonly SingleThreadSynchronizationContext synchronizationContext;

        public SingleThreadUsage() {
            this.synchronizationContext = new SingleThreadSynchronizationContext("Client");
            this.EventManager = new EventManager(new MessageBus());
        }

        private IEventManager EventManager { get; set; }

        public async Task Run() {
            this.synchronizationContext.Run();

            SynchronizationContext.SetSynchronizationContext(this.synchronizationContext);

            using (new MockTransport(this.EventManager))
                await this.RunClient();
        }

        public void Dispose() {
            this.synchronizationContext.Complete();
        }

        private async Task RunClient() {
            var connectRequestedEvent = new ConnectRequested {Address = "http://localhost"};

            // We can raise event before starting waiting for result, because it will be processed in same synchronization context (after await call)
            this.EventManager.RaiseEvent(connectRequestedEvent);
            // Thread: Main
            // Waits for connected or cancel event. Address of connect event must be http://localhost. 
            // This call blocks execution, so it'll continue in client thread
            var @event = await this.EventManager.WaitFor<Connected, CancelRequested>(TimeSpan.FromMilliseconds(50),
                e => !(e is Connected) || ((Connected) e).Address == "http://localhost");
            // Thread: Client
            // Expected result: Connected
            CheckStatus(@event);

            connectRequestedEvent.Delay = TimeSpan.FromMilliseconds(100);
            this.EventManager.RaiseEvent(connectRequestedEvent);
            @event = await this.EventManager.WaitFor<Connected, CancelRequested>(TimeSpan.FromMilliseconds(50));
            // Expected result: Timeout, reason: connect delay > 50 ms
            CheckStatus(@event);

            this.EventManager.RaiseEvent(connectRequestedEvent);
            var eventWaitTask = this.EventManager.WaitFor<Connected, CancelRequested>(TimeSpan.FromMilliseconds(50));
            this.EventManager.RaiseEvent(new CancelRequested());

            @event = await eventWaitTask;
            // Expected result: cancelled, reason: cancel event raised before connected event
            CheckStatus(@event);
        }

        private static void CheckStatus(IEvent @event) {
            if (@event == null)
                Log.Warn("Connection timeout");
            else if (@event is CancelRequested)
                Log.Warn("Connection cancelled");
            else
                Log.Warn("Connected {0}", ((Connected) @event).Address);
        }
    }
}