using System;
using System.Threading.Tasks;
using Eventing.Library;
using NUnit.Framework;

namespace Eventing.Tests {
    [TestFixture]
    public class EventManagerTests : TestBase {
        private class TestMessage : IEvent {
        }

        private class SecondTestMessage : IEvent {
        }

        [Test]
        public void TerminatesTaskByTimeout() {
            var task = this.EventManager.WaitFor<TestMessage>(TimeSpan.FromMilliseconds(5));
            var completed = task.Wait(TimeSpan.FromSeconds(1));

            Assert.True(completed);
            Assert.IsNull(task.Result);
            Assert.AreEqual(0, this.MessageBus.GetSubscriptions().Count);
        }

        [Test]
        public async Task WaitsForAppropriateMessage() {
            var task = this.EventManager.WaitFor<SecondTestMessage>();

            this.MessageBus.Send(new TestMessage());
            var testMessage = new SecondTestMessage();
            this.MessageBus.Send(testMessage);

            this.React();

            var result = await task;

            Assert.AreEqual(result, testMessage);
        }

        [Test]
        public async Task WaitsForFirstMessage() {
            var task = this.EventManager.WaitFor<TestMessage, SecondTestMessage>();
            var testMessage = new TestMessage();
            this.MessageBus.Send(testMessage);
            this.React();

            var result = await task;

            Assert.AreEqual(result, testMessage);
        }

        [Test]
        public async Task WaitsForMessage() {
            var task = this.EventManager.WaitFor<TestMessage>();
            var testMessage = new TestMessage();

            this.MessageBus.Send(testMessage);
            this.React();

            var result = await task;

            Assert.AreEqual(result, testMessage);
        }
    }
}