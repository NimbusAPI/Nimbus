using System;
using System.Configuration;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Nimbus;
using Nimbus.Configuration;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.Logging;
using Nimbus.Unity.Configuration;

namespace PingPong.Unity
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var container = new UnityContainer();
            container.RegisterType<IPinger, Pinger>();
            
            SetUpBus(container);

            Console.WriteLine("Enter some text to have it ponged back at you. Type 'exit' to quit...");

            var exit = new ManualResetEvent(false);
            Task.Run(() => Start(container.Resolve<IPinger>(), exit));
            exit.WaitOne();
        }

        private static async void Start(IPinger pinger, ManualResetEvent exit)
        {
            while (true)
            {
                var input = Console.ReadLine();
                if (input.ToLowerInvariant() == "exit")
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

        private static void SetUpBus(IUnityContainer container)
        {
            //TODO: Set up your own connection string in app.config
            var connectionString = ConfigurationManager.AppSettings["AzureConnectionString"];

            // You'll want a logger. There's a ConsoleLogger and a NullLogger if you really don't care. You can roll your
            // own by implementing the ILogger interface if you want to hook it to an existing logging implementation.
            container.RegisterType<ILogger, ConsoleLogger>();

            // This is how you tell Nimbus where to find all your message types and handlers.
            var typeProvider = new AssemblyScanningTypeProvider(Assembly.GetExecutingAssembly());

            var bus = new BusBuilder().Configure()
                                      .WithConnectionString(connectionString)
                                      .WithNames("PingPong.Unity", Environment.MachineName)
                                      .WithTypesFrom(typeProvider)
                                      .WithDefaultTimeout(TimeSpan.FromSeconds(5))
                                      .WithUnityDependencyResolver(typeProvider, container)
                                      .Build();

            bus.Start().Wait();

            container.RegisterInstance<IBus>(bus);
        }
    }
}