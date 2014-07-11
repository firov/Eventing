using System.Threading.Tasks;
using Eventing.ExamplesOfTests.Events;
using NUnit.Framework;

namespace Eventing.ExamplesOfTests {
    /// <summary>
    ///     This test demonstrates how to test application that uses Eventing
    ///     All code executes sequently in one thread
    /// </summary>
    [TestFixture]
    public class TestCase : TestBase {
        [Test]
        public async Task ClientDoesWork() {
            var client = new Client(this.EventManager);
            var doWorkAwaitable = client.DoWork();

            this.EventManager.RaiseEvent(new Connected());

            // We can debug and step into 
            this.React();

            await doWorkAwaitable;

            Assert.AreEqual(true, client.Connected);
        }
    }
}