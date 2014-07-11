using System;
using System.Threading.Tasks;
using Eventing.Examples.Events;
using Eventing.Library;
using NLog;

namespace Eventing.Examples.Impl {
    internal class MockTransport : IDisposable {
        IEventManager EventManager { get; set; }
        private static readonly Logger Log = LogManager.GetLogger("Eventing.Examples");

        public MockTransport(IEventManager eventManager) {
            this.EventManager = eventManager;
            this.EventManager.StartReceiving<ConnectRequested>(async @event => await this.ConnectRequested(@event));

            Log.Info("Mock transport initialized");
        }

        private async Task ConnectRequested(ConnectRequested @event) {
            // Connect logic here
            await Task.Delay(@event.Delay);

            // Connection succeeded
            this.EventManager.RaiseEvent(new Connected{Address = @event.Address});
        }

        public void Dispose() {
            // Unsubscribes from all events
            this.EventManager.StopReceiving(this);
        }
    }
}