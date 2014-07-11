Eventing
========

Eventing is a library for asynchronous event-driven programming.

- [Homepage](https://github.com/firov/Eventing)
- [NuGet Package](https://www.nuget.org/packages/Eventing/)
- [License](LICENSE)

### Overview

Eventing provides you an easy way to write and test asynchronous code using new .net 4.5 async/await feature. The main idea is to replace method calls with raising/waiting events and threads with synchronization context.  This allows to use multithreading while testing code in single thread. At the same time async/await helps us highly increase thread utilization. You also can remove locks from your code, since you determ in which thread code will execute. 

### Usage
*See Eventing.Examples and Eventing.ExamplesOfTests for more information*

**Initialization**

Create synchronization context and event manager:
```
var synchronizationContext = new SingleThreadSynchronizationContext("Client");
var EventManager = new EventManager(new MessageBus());
```
then switch to created synchronization context:
```
SynchronizationContext.SetSynchronizationContext(this.synchronizationContext);
```
or post a job to it
```
synchronizationContext.Post(async o => await this.RunClient(), null);
```

**Raising events**

To raise an event create a class that implements *IEvent*
```
internal class ConnectRequested : IEvent {
        public string Address { get; set; }
    }
```
then use *RaiseEvent*:
```
this.EventManager.RaiseEvent(new ConnectRequested {Address = "http://localhost"});
```

**Waiting events**

To wait for an event use *WaitFor* method. With template arguments specify event types you are waiting for:
```
var @event = await this.EventManager.WaitFor<Connected, CancelRequested>(TimeSpan.FromMilliseconds(50));
```
After this call *@event* will contain instance of *Connected* or *CancelRequested* class or null in case of timeout. Code execution will continue in same thread it was before *await* call (If it was in single thread synchronization context).

**Receiving events**

To start receiving events use *StartReceiving*:
```
eventManager.StartReceiving<ConnectRequested>(@event => this.ConnectRequested(@event));
```
When *ConnectRequested* raised it will be processed by *ConnectRequested* method in same synchronization context *StartReceiving* called. Using *SingleThreadSynchronizationContext* you can restrict all operations in class by one thread and forget about locks.

To stop receiving events by *this* instance use *StopReceiving*:
```
eventManager.StopReceiving(this);
```
