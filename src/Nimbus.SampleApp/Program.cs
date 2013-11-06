using System;
using System.Reflection;
using Autofac;
using Nimbus.Autofac;
using Nimbus.Configuration;
using Nimbus.InfrastructureContracts;
using Serilog;

namespace Nimbus.SampleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            using (var container = CreateContainer())
            {
                var deepThought = container.Resolve<DeepThought>();
                deepThought.ComputeTheAnswer().Wait();
                Console.ReadKey();
            }
        }

        private static IContainer CreateContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<DeepThought>();

            var handlerTypesProvider = new AssemblyScanningTypeProvider(Assembly.GetExecutingAssembly());

            builder.Register(i => new LoggerConfiguration()
                                    .MinimumLevel.Debug()
                                    .WriteTo.ColoredConsole()
                                    .CreateLogger())
                   .As<ILogger>();

            builder.RegisterNimbus(handlerTypesProvider);
            builder.Register(c => new BusBuilder()
                                      .Configure()
                                      .WithConnectionString(@"Endpoint=sb://cacofonix/NimbusTest;StsEndpoint=https://cacofonix:9355/NimbusTest;RuntimePort=9354;ManagementPort=9355;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=YwlCk9bO9PS8VUZJizoq1ILa9v7I0IM9cnvLEaH15Kc=")
                                      .WithInstanceName(Environment.MachineName + ".MyApp")
                                      .WithTypesFrom(handlerTypesProvider)
                                      .WithEventBroker(c.Resolve<IEventBroker>())
                                      .WithCommandBroker(c.Resolve<ICommandBroker>())
                                      .WithRequestBroker(c.Resolve<IRequestBroker>())
                                      .WithSeriLogger(c.Resolve<ILogger>())
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