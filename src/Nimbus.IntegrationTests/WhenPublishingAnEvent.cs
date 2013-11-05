using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Nimbus.Configuration;
using Nimbus.InfrastructureContracts;
using Nimbus.IntegrationTests.Extensions;
using Nimbus.IntegrationTests.MessageContracts;

namespace Nimbus.IntegrationTests
{
    [TestFixture]
    public class WhenPublishingAnEvent : SpecificationFor<Bus>
    {
        private ICommandBroker _commandBroker;
        private IRequestBroker _requestBroker;
        private IEventBroker _eventBroker;
        private ITimeoutBroker _timeoutBroker;

        public override Bus Given()
        {
            _commandBroker = Substitute.For<ICommandBroker>();
            _requestBroker = Substitute.For<IRequestBroker>();
            _eventBroker = Substitute.For<IEventBroker>();
            _timeoutBroker = Substitute.For<ITimeoutBroker>();

            var typeProvider = new AssemblyScanningTypeProvider(typeof (SomeEvent).Assembly);

            var bus = new BusBuilder().Configure()
                                      .WithInstanceName(Environment.MachineName + ".MyTestSuite")
                                      .WithConnectionString(CommonResources.ConnectionString)
                                      .WithTypesFrom(typeProvider)
                                      .WithCommandBroker(_commandBroker)
                                      .WithTimeoutBroker(_timeoutBroker)
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

            TimeSpan.FromSeconds(5).SleepUntil(() => _eventBroker.ReceivedCalls().Any());

            Subject.Stop();
        }

        [Test]
        public void SomethingShouldHappen()
        {
            _eventBroker.Received().Publish(Arg.Any<SomeEvent>());
        }
    }
}