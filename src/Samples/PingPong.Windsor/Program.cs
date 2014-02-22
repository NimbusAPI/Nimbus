using System;
using System.Configuration;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Castle.Facilities.Startable;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Nimbus;
using Nimbus.Configuration;
using Nimbus.Infrastructure;
using Nimbus.Logger.Serilog;
using Serilog;
using ILogger = Nimbus.ILogger;

namespace PingPong.Windsor
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.ColoredConsole()
                .MinimumLevel.Debug()
                .CreateLogger();

            var container = new WindsorContainer();
            container.AddFacility<StartableFacility>();
            container.Register(Component.For<IPinger>().ImplementedBy<Pinger>().LifestyleSingleton());

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

        private static void SetUpBus(IWindsorContainer container)
        {
            //TODO: Set up your own connection string in app.config
            var connectionString = ConfigurationManager.AppSettings["AzureConnectionString"];

            // You'll want a logger. There's a ConsoleLogger and a NullLogger if you really don't care. You can roll your
            // own by implementing the ILogger interface if you want to hook it to an existing logging implementation.
            container.Register(Component.For<ILogger>().ImplementedBy<SerilogStaticLogger>().LifestyleSingleton());

            // This is how you tell Nimbus where to find all your message types and handlers.
            var typeProvider = new AssemblyScanningTypeProvider(Assembly.GetExecutingAssembly());

            container.RegisterNimbus(typeProvider);
            container.Register(Component.For<IBus>().ImplementedBy<Bus>().UsingFactoryMethod<IBus>(() => new BusBuilder()
                                                                                                       .Configure()
                                                                                                       .WithConnectionString(connectionString)
                                                                                                       .WithNames("PingPong.Windsor", Environment.MachineName)
                                                                                                       .WithTypesFrom(typeProvider)
                                                                                                       .WithWindsorDefaults(container)
                                                                                                       .Build())
                                        .LifestyleSingleton()
                                        .StartUsingMethod("Start")
                );
        }
    }
}