using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Nimbus.Configuration;
using Nimbus.InfrastructureContracts;
using Nimbus.IntegrationTests.EventTests.MessageContracts;
using Nimbus.IntegrationTests.Extensions;

namespace Nimbus.IntegrationTests.EventTests
{
    [TestFixture]
    public class WhenPublishingAnEventThatWeHandleViaCompetitionAndMulticast: SpecificationFor<Bus>
    {
        private ICommandBroker _commandBroker;
        private IRequestBroker _requestBroker;
        private IMulticastEventBroker _multicastEventBroker;
        private ICompetingEventBroker _competingEventBroker;

        public override Bus Given()
        {
            _commandBroker = Substitute.For<ICommandBroker>();
            _requestBroker = Substitute.For<IRequestBroker>();
            _multicastEventBroker = Substitute.For<IMulticastEventBroker>();
            _competingEventBroker = Substitute.For<ICompetingEventBroker>();

            var typeProvider = new AssemblyScanningTypeProvider(typeof(SomeEventWeOnlyHandleViaMulticast).Assembly);

            var bus = new BusBuilder().Configure()
                                      .WithNames("MyTestSuite", Environment.MachineName)
                                      .WithConnectionString(CommonResources.ConnectionString)
                                      .WithTypesFrom(typeProvider)
                                      .WithCommandBroker(_commandBroker)
                                      .WithRequestBroker(_requestBroker)
                                      .WithMulticastEventBroker(_multicastEventBroker)
                                      .WithCompetingEventBroker(_competingEventBroker)
                                      .Build();
            bus.Start();
            return bus;
        }

        public override void When()
        {
            var myEvent = new SomeEventWeHandleViaMulticastAndCompetition();
            Subject.Publish(myEvent).Wait();

            TimeSpan.FromSeconds(5).SleepUntil(() => _competingEventBroker.ReceivedCalls().Any());

            Subject.Stop();
        }

        [Test]
        public void TheCompetingEventBrokerShouldReceiveTheEvent()
        {
            _competingEventBroker.Received().Publish(Arg.Any<SomeEventWeHandleViaMulticastAndCompetition>());
        }

        [Test]
        public void TheMulticastEventBrokerShouldReceiveTheEvent()
        {
            _multicastEventBroker.Received().Publish(Arg.Any<SomeEventWeHandleViaMulticastAndCompetition>());
        }
    }
}