using System;
using System.Reflection;
using Nimbus.Configuration;
using Nimbus.HandlerFactories;
using Nimbus.Infrastructure;

namespace Nimbus.SampleApp
{
    internal class OtherProgramWithoutAutofac
    {
        public void DoFoo()
        {
            var typeProvider = new AssemblyScanningTypeProvider(Assembly.GetExecutingAssembly());
            IMulticastEventHandlerFactory multicastEventHandlerFactory = new DefaultMessageHandlerFactory(typeProvider);

            var bus = new BusBuilder().Configure()
                                      .WithConnectionString("foo")
                                      .WithNames("MyApp", Environment.MachineName)
                                      .WithTypesFrom(typeProvider)
                                      .WithMulticastEventHandlerFactory(multicastEventHandlerFactory)
                                      .Build();
        }
    }
}