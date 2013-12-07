using System;
using System.Configuration;
using System.Reflection;
using Autofac;
using Nimbus.Autofac.Configuration;
using Nimbus.Configuration;
using Nimbus.Infrastructure;
using Nimbus.Logger;

namespace Nimbus.SampleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            using (var container = CreateContainer())
            {
                var heartbeat = container.Resolve<Heartbeat>();
                heartbeat.Run();


                var deepThought = container.Resolve<DeepThought>();
                deepThought.ComputeTheAnswer().Wait();
                Console.ReadKey();
            }
        }

        private static IContainer CreateContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<DeepThought>();

            builder.RegisterType<Heartbeat>().SingleInstance();


            //TODO: Set up your own connection string in app.config
            var connectionString = ConfigurationManager.AppSettings["AzureConnectionString"];

            builder.RegisterType<ConsoleLogger>()
                   .AsImplementedInterfaces()
                   .SingleInstance();

            var handlerTypesProvider = new AssemblyScanningTypeProvider(Assembly.GetExecutingAssembly());
            builder.RegisterNimbus(handlerTypesProvider);
            builder.Register(componentContext => new BusBuilder()
                                 .Configure()
                                 .WithConnectionString(connectionString)
                                 .WithNames("MyApp", Environment.MachineName)
                                 .WithTypesFrom(handlerTypesProvider)
                                 .WithAutofacDefaults(componentContext)
                                 .Build())
                   .As<IBus>()
                   .AutoActivate()
                   .OnActivated(c => c.Instance.Start())
                   .SingleInstance();

            var container = builder.Build();
            return container;
        }
    }
}