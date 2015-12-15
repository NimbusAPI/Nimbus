using System;
using System.Configuration;
using System.Reflection;
using Autofac;
using Nimbus;
using Nimbus.Configuration;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.Logging;
using Nimbus.Transports.WindowsServiceBus;
using Pizza.Maker.Messages;
using Pizza.Ordering.Messages;

namespace Pizza.WaitTimeService
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<WaitTimeCounter>().AsImplementedInterfaces().SingleInstance();

            SetUpBus(builder);

            var container = builder.Build();

            Console.WriteLine("Wait time service started. Any key to quit");
            while (true)
            {
                var input = Console.ReadKey();
                return;
            }
        }

        private static void SetUpBus(ContainerBuilder builder)
        {
            //TODO: Set up your own connection string in app.config
            var connectionString = ConfigurationManager.AppSettings["AzureConnectionString"];

            // You'll want a logger. There's a ConsoleLogger and a NullLogger if you really don't care. You can roll your
            // own by implementing the ILogger interface if you want to hook it to an existing logging implementation.
            builder.RegisterType<ConsoleLogger>()
                   .AsImplementedInterfaces()
                   .SingleInstance();

            // This is how you tell Nimbus where to find all your message types and handlers.
            var typeProvider = new AssemblyScanningTypeProvider(Assembly.GetExecutingAssembly(), typeof (NewOrderRecieved).Assembly, typeof (HowLongDoPizzasTakeRequest).Assembly);

            builder.RegisterNimbus(typeProvider);
            builder.Register(componentContext => new BusBuilder()
                                 .Configure()
                                 .WithTransport(new WindowsServiceBusTransportConfiguration())
                                 .WithConnectionString(connectionString)
                                 .WithNames("WaitTime", Environment.MachineName)
                                 .WithTypesFrom(typeProvider)
                                 .WithAutofacDefaults(componentContext)
                                 .Build())
                   .As<IBus>()
                   .AutoActivate()
                   .OnActivated(c => c.Instance.Start().Wait())
                   .SingleInstance();
        }
    }
}