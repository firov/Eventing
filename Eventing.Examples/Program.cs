using System;
using System.Threading;
using Eventing.Examples.Impl;
using NLog;
using NLog.Config;

namespace Eventing.Examples {
    internal class Program {
        private static void Main(string[] args) {
            LogManager.Configuration = XmlLoggingConfiguration.AppConfig;

            var log = LogManager.GetLogger("Eventing.Examples");

            Thread.CurrentThread.Name = "Main";

            log.Warn("SingleThreadUsage");

            using (var example = new SingleThreadUsage())
                RunExample(example);

            log.Warn("MultiThreadUsage");

            using (var example = new MultiThreadUsage())
                RunExample(example);

            Console.ReadKey();
        }

        private static void RunExample(IExample example) {
            example.Run().Wait();
        }
    }
}