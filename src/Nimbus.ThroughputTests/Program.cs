using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Nimbus.IntegrationTests;

namespace Nimbus.ThroughputTests
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            MainAsync().Wait();
        }

        private static async Task MainAsync()
        {
            var connstring = CommonResources.ConnectionString;

            var messageCount = 50;

            var broker = new FakeBroker(messageCount);

            var bus = new BusBuilder(connstring, broker, broker, broker, new Type[] {}, new Type[] {}, new[] {typeof (MyEvent)}).Build();

            Console.WriteLine("Press any key to start");
            Console.ReadKey();

            bus.Start();

            var sw = Stopwatch.StartNew();
            for (var i = 0; i < messageCount; i++)
            {
                await bus.Publish(new MyEvent());
                Console.Write(".");
            }
            Console.WriteLine();
            Console.WriteLine("Finished sending messages. Waiting for them to all find their way back...");
            broker.WaitUntilDone();
            sw.Stop();

            Console.WriteLine("All done. Took {0} milliseconds to process {1} messages", sw.ElapsedMilliseconds, messageCount);
            var messagesPerSecond = (double) messageCount/sw.Elapsed.TotalSeconds;
            Console.WriteLine("Average throughput: {0} messages/second", messagesPerSecond);
            Console.ReadKey();
        }
    }
}