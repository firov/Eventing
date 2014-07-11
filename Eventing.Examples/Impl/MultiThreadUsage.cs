using System;
using System.Threading;
using System.Threading.Tasks;
using Eventing.Examples.Events;
using Eventing.Library;
using Eventing.Library.Impl;
using NLog;

namespace Eventing.Examples.Impl {
    internal class MultiThreadUsage : IExample {
        private static readonly Logger Log = LogManager.GetLogger("Eventing.Examples");

        private readonly SingleThreadSynchronizationContext clientSynchronizationContext;
        private readonly SingleThreadSynchronizationContext serverSynchronizationContext;

        public MultiThreadUsage() {
            this.serverSynchronizationContext = new SingleThreadSynchronizationContext("Server thread");
            this.clientSynchronizationContext = new SingleThreadSynchronizationContext("Client thread");
            this.EventManager = new EventManager(new MessageBus());
        }

        private IEventManager EventManager { get; set; }

        public async Task Run() {
            this.serverSynchronizationContext.Run();
            this.clientSynchronizationContext.Run();

            SynchronizationContext.SetSynchronizationContext(this.clientSynchronizationContext);

            this.serverSynchronizationContext.Post(async o => await this.RunServer(), null);

            await this.RunClient();
        }

        public void Dispose() {
            this.clientSynchronizationContext.Complete();
            this.serverSynchronizationContext.Complete();
        }

        private async Task RunServer() {
            // It is allowed to listen and wait for event simultaneously
            // Have to explicitly specify listner(this), because anonymous function does not provide this information
            this.EventManager.StartReceiving<ConnectRequested>(e => Log.Warn("Connect requested."), this);

            // Can't here raise ServerFound and then wait for ConnectRequested because all client logic executed in separate thread
            // and there is possobility thet client will raise ConnectRequested before we starts to wait for it
            // so, creates awaitable task first
            var eventTask = this.EventManager.WaitFor<ConnectRequested>(TimeSpan.FromSeconds(1));

            this.EventManager.RaiseEvent(new ServerFound());

            // and then waits for it
            var @event = await eventTask as ConnectRequested;
            if (@event == null) {
                Log.Warn("Timeout");
                return;
            }

            Log.Warn("Processing client request {0}", @event.Address);

            this.EventManager.RaiseEvent(new Connected());
        }

        private async Task RunClient() {
            // Waits for server
            await this.EventManager.WaitFor<ServerFound>();

            this.EventManager.RaiseEvent(new ConnectRequested {Address = "http://localhost"});

            var @event = await this.EventManager.WaitFor<Connected, CancelRequested>(TimeSpan.FromSeconds(1));
            // Expected result: Timeout, reason: connect delay > 50 ms
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