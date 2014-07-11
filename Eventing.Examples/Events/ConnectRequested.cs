using System;

namespace Eventing.Examples.Events {
    internal class ConnectRequested : BasicEvent {
        public string Address { get; set; }
        public TimeSpan Delay { get; set; }
    }
}