using System;
using System.Reflection;
using System.Threading;
using NSubstitute;
using Nimbus.Configuration;
using Nimbus.InfrastructureContracts;

namespace Nimbus.IntegrationTests
{
    public class WhenPublishingAnEvent : SpecificationFor<Bus>
    {
        private ICommandBroker _commandBroker;
        private IRequestBroker _requestBroker;
        private IEventBroker _eventBroker;

        public override Bus Given()
        {
            _commandBroker = Substitute.For<ICommandBroker>();
            _requestBroker = Substitute.For<IRequestBroker>();
            _eventBroker = Substitute.For<IEventBroker>();

            var typeProvider = new AssemblyScanningTypeProvider(Assembly.GetExecutingAssembly());

            var bus = new BusBuilder().Configure()
                                      .WithInstanceName(Environment.MachineName + ".MyTestSuite")
                                      .WithConnectionString(CommonResources.ConnectionString)
                                      .WithHandlerTypesFrom(typeProvider)
                                      .WithCommandBroker(_commandBroker)
                                      .WithRequestBroker(_requestBroker)
                                      .WithEventBroker(_eventBroker)
                                      .Build();
            bus.Start();
            return bus;
        }

        public override void When()
        {
            var myEvent = new SomeEvent();
            Subject.Publish(myEvent).Wait();

            Thread.Sleep(2000);

            Subject.Stop();
        }

        [Then]
        public void SomethingShouldHappen()
        {
            _eventBroker.Received().Publish(Arg.Any<SomeEvent>());
        }
    }
}