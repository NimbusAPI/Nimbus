using System;
using System.Configuration;
using Autofac;
using Nimbus;
using Nimbus.Configuration;
using Nimbus.Infrastructure;
using Nimbus.Logger;
using Pizza.Maker.Messages;
using Pizza.Ordering.Messages;

namespace Pizza.RetailWeb.AutofacModules
{
    public class BusModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            var connectionString = ConfigurationManager.AppSettings["AzureConnectionString"];

            // You'll want a logger. There's a ConsoleLogger and a NullLogger if you really don't care. You can roll your
            // own by implementing the ILogger interface if you want to hook it to an existing logging implementation.
            builder.RegisterType<ConsoleLogger>()
                   .AsImplementedInterfaces()
                   .SingleInstance();

            // This is how you tell Nimbus where to find all your message types and handlers.
            var pizzaOrderingMessagesAssembly = typeof (HowLongDoPizzasTakeRequest).Assembly;
            var pizzaMakerMessagesAssembly = typeof (PizzaIsReady).Assembly;

            var handlerTypesProvider = new AssemblyScanningTypeProvider(ThisAssembly, pizzaOrderingMessagesAssembly, pizzaMakerMessagesAssembly);

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
        }
    }
}