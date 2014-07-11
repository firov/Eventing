using System;
using System.Threading.Tasks;

namespace Eventing.Examples {
    internal interface IExample : IDisposable {
        Task Run();
    }
}