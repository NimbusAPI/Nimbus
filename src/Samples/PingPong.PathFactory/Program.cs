using System;
using System.Configuration;
using System.Reflection;
using System.Threading;
using Nimbus;
using Nimbus.Configuration;
using Nimbus.Infrastructure;

namespace PingPong.PathGenerator
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var bus = SetUpBus();

            Console.WriteLine("Enter some text to have it ponged back at you. Type 'exit' to quit...");

            var exit = new ManualResetEvent(false);
            Start(new Pinger(bus), exit);

            exit.WaitOne();
        }

        private static async void Start(IPinger pinger, ManualResetEvent exit)
        {
            while (true)
            {
                var input = Console.ReadLine();
                if (input != null && input.ToLowerInvariant() == "exit")
                {
                    exit.Set();
                }
                else
                {
                    var pong = await pinger.Ping(input);
                    Console.WriteLine(pong);
                }
            }
        }

        private static IBus SetUpBus()
        {
            //TODO: Set up your own connection string in app.config
            var connectionString = ConfigurationManager.AppSettings["AzureConnectionString"];

            // This is how you tell Nimbus where to find all your message types and handlers.
            var typeProvider = new AssemblyScanningTypeProvider(Assembly.GetExecutingAssembly());

            var bus = new BusBuilder()
                .Configure()
                .WithConnectionString(connectionString)
                .WithNames("PingPong.pathGenerator", Environment.MachineName)
                .WithTypesFrom(typeProvider)
                .WithJsonSerializer()
                .WithDeflateCompressor()
                .WithPathGenerator(new CustomPathGenerator())
                .Build();
            bus.Start().Wait();

            return bus;
        }
    }
}