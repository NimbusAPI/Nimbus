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
            IEventBroker eventBroker = new DefaultEventBroker(Assembly.GetExecutingAssembly());
            var typeProvider = new AssemblyScanningTypeProvider(Assembly.GetExecutingAssembly());

            var bus = new BusBuilder().Configure()
                                      .WithConnectionString("foo")
                                      .WithInstanceName("MyApp")
                                      .WithTypesFrom(typeProvider)
                                      .WithEventBroker(eventBroker)
                                      .Build();
        }
    }
}