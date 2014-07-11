using System.Threading.Tasks;
using Eventing.ExamplesOfTests.Events;
using Eventing.Library;
using NLog;

namespace Eventing.ExamplesOfTests {
    internal class Client {
        private static readonly Logger Log = LogManager.GetLogger("Tests");
        private readonly IEventManager eventManager;

        public Client(IEventManager eventManager) {
            this.eventManager = eventManager;
        }

        public bool Connected { get; private set; }

        public async Task DoWork() {
            await this.eventManager.WaitFor<Connected>();

            Log.Info("Connected");
            this.Connected = true;
        }
    }
}