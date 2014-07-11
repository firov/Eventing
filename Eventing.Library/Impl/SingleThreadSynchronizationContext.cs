using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Eventing.Library.Impl {
    /// <summary>
    ///     Implementation of synchronization context which uses one thread for all operations
    /// </summary>
    public sealed class SingleThreadSynchronizationContext : SynchronizationContext {
        private readonly object completionSyncRoot = new object();

        private readonly BlockingCollection<KeyValuePair<SendOrPostCallback, object>> operationsQueue =
            new BlockingCollection<KeyValuePair<SendOrPostCallback, object>>();

        private readonly Thread workerThread;

        public SingleThreadSynchronizationContext(string threadName = "Synchronization thread") {
            this.workerThread = new Thread(this.RunOnCurrentThread) {
                Name = threadName,
                IsBackground = true
            };
        }

        /// <summary>
        ///     Starts synchronization context execution
        /// </summary>
        public void Run() {
            this.workerThread.Start();
        }

        /// <summary>
        ///     Dispatches an asynchronous message to a synchronization context.
        /// </summary>
        /// <param name="callback">The System.Threading.SendOrPostCallback delegate to call</param>
        /// <param name="state">The object passed to the delegate</param>
        public override void Post(SendOrPostCallback callback, object state) {
            if (callback == null)
                throw new ArgumentNullException("callback");

            lock (this.completionSyncRoot) {
                if (this.operationsQueue.IsAddingCompleted)
                    return;

                this.operationsQueue.Add(new KeyValuePair<SendOrPostCallback, object>(callback, state));
            }
        }

        /// <summary>
        ///     Executes all pending operations. Usefull for unit tests
        /// </summary>
        /// <returns>True if atleast one operation completed</returns>
        public bool RunAllOperations() {
            if (this.operationsQueue.Count == 0)
                return false;

            KeyValuePair<SendOrPostCallback, object> workItem;

            while (this.operationsQueue.TryTake(out workItem))
                workItem.Key(workItem.Value);

            return true;
        }

        /// <summary>
        ///     Dispatches a synchronous message to a synchronization context.
        /// </summary>
        /// <param name="callback">The System.Threading.SendOrPostCallback delegate to call</param>
        /// <param name="state">The object passed to the delegate</param>
        public override void Send(SendOrPostCallback callback, object state) {
            throw new NotSupportedException("Synchronously sending is not supported.");
        }

        /// <summary>
        ///     Completes synchronization context, releasing its thread.
        ///     After completion context will ignore new operations.
        /// </summary>
        public void Complete() {
            lock (this.completionSyncRoot) {
                this.operationsQueue.CompleteAdding();
            }
        }

        public event EventHandler<object> UnhandledException;

        private bool OnUnhandledException(Exception exception) {
            var handler = this.UnhandledException;

            if (handler == null)
                return false;

            handler(this, exception);
            return true;
        }

        private void RunOnCurrentThread() {
            SetSynchronizationContext(this);

            foreach (var operation in this.operationsQueue.GetConsumingEnumerable())
                try {
                    operation.Key(operation.Value);
                } catch (Exception exception) {
                    if (!this.OnUnhandledException(exception))
                        throw;
                }
        }
    }
}