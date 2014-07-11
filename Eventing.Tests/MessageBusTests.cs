using Eventing.Library;
using Eventing.Library.Impl;
using NUnit.Framework;

namespace Eventing.Tests {
    [TestFixture]
    internal class MessageBusTests : TestBase {
        private class TestMessage {
        }

        [Test]
        public void AutomaticallyUnsubscribes() {
            var received = false;
            var testSubscription = MessageSubscription.Create<TestMessage>(_ => { received = true; }, this,
                UnsubscribePolicy.Auto);
            this.MessageBus.Subscribe(testSubscription);
            this.MessageBus.Send(new TestMessage());
            this.React();
            Assert.IsTrue(received);
        }

        [Test]
        public void SendsMessage() {
            var received = false;
            var testSubscription = MessageSubscription.Create<TestMessage>(_ => { received = true; }, this);
            this.MessageBus.Subscribe(testSubscription);
            this.MessageBus.Send(new TestMessage());
            this.React();
            Assert.IsTrue(received);
        }
    }
}