using System;
using System.Reflection;
using Autofac;
using Nimbus.Configuration;
using Nimbus.InfrastructureContracts;

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

            builder.RegisterNimbus(handlerTypesProvider);
            builder.Register(c => new BusBuilder()
                                      .Configure()
                                      .WithConnectionString(@"Endpoint=sb://nimbustest.servicebus.windows.net/;SharedAccessKeyName=Demo;SharedAccessKey=bQppKwhg3xfBpIYqTAWcn9fC5HK1F2eh7G+AHb66jis=")
                                      .WithInstanceName(Environment.MachineName + ".MyApp")
                                      .WithHandlerTypesFrom(handlerTypesProvider)
                                      .WithEventBroker(c.Resolve<IEventBroker>())
                                      .WithCommandBroker(c.Resolve<ICommandBroker>())
                                      .WithRequestBroker(c.Resolve<IRequestBroker>())
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