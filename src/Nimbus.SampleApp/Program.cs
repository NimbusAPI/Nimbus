using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Nimbus.Autofac;

namespace Nimbus.SampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ContainerBuilder();

            var bus = new BusBuilder()
                .WithConnectionString("foo")
                .WithInstanceName("MyApp")
                //.WithEventBroker(() => new FakeBroker())
                .WithAutofac(builder)
                .Build();

            builder.RegisterInstance(bus).As<IBus>().SingleInstance();

            var container = builder.Build();
            bus.Start();

        }
    }

    public static class AutofacBusBuilderExtensions
    {
        public static BusBuilder WithAutofac(this BusBuilder busBuilder, ContainerBuilder containerBuilder)
        {

            var registrar = new AutofacHandlerRegistration(containerBuilder);
            busBuilder.RegisterHandlers(registrar);

            containerBuilder.RegisterType<AutofacEventBroker>().As<IEventBroker>();
            containerBuilder.RegisterType<AutofacCommandBroker>().As<ICommandBroker>();
            containerBuilder.RegisterType<AutofacRequestBroker>().As<IRequestBroker>();

            
            return busBuilder;
        }
    }
}
