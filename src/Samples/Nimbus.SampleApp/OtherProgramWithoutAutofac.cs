using System;
using System.Reflection;
using Nimbus.Configuration;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;

namespace Nimbus.SampleApp
{
    internal class OtherProgramWithoutAutofac
    {
        public void DoFoo()
        {
            var typeProvider = new AssemblyScanningTypeProvider(Assembly.GetExecutingAssembly());
            IMulticastEventBroker multicastEventBroker = new DefaultMessageBroker(typeProvider);

            var bus = new BusBuilder().Configure()
                                      .WithConnectionString("foo")
                                      .WithNames("MyApp", Environment.MachineName)
                                      .WithTypesFrom(typeProvider)
                                      .WithMulticastEventBroker(multicastEventBroker)
                                      .Build();
        }
    }
}