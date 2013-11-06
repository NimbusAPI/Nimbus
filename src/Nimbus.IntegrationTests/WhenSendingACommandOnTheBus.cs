using System;
using System.Linq;
using System.Reflection;
using NSubstitute;
using NUnit.Framework;
using Nimbus.Configuration;
using Nimbus.InfrastructureContracts;
using Nimbus.IntegrationTests.Extensions;
using Nimbus.IntegrationTests.MessageContracts;

namespace Nimbus.IntegrationTests
{
    [TestFixture]
    public class WhenSendingACommandOnTheBus : SpecificationFor<Bus>
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

            var typeProvider = new AssemblyScanningTypeProvider(Assembly.GetExecutingAssembly());

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
            var someCommand = new SomeCommand();
            Subject.Send(someCommand).Wait();
            TimeSpan.FromSeconds(1).SleepUntil(() => _commandBroker.ReceivedCalls().Any());

            Subject.Stop();
        }

        [Test]
        public void TheCommandBrokerShouldReceiveThatCommand()
        {
            _commandBroker.Received().Dispatch(Arg.Any<SomeCommand>());
        }
    }
}