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
    public class WhenSendingATimeoutOnTheBus : SpecificationFor<Bus>
    {
        private ICommandBroker _commandBroker;
        private IRequestBroker _requestBroker;
        private IEventBroker _eventBroker;
        private ITimeoutBroker _timeoutBroker;

        public override Bus Given()
        {
            _commandBroker = Substitute.For<ICommandBroker>();
            _timeoutBroker = Substitute.For<ITimeoutBroker>();
            _requestBroker = Substitute.For<IRequestBroker>();
            _eventBroker = Substitute.For<IEventBroker>();

            var typeProvider = new AssemblyScanningTypeProvider(Assembly.GetExecutingAssembly());

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
            var someTimeout = new SomeTimeout();
            Subject.Defer(TimeSpan.FromSeconds(1),someTimeout).Wait();
            TimeSpan.FromSeconds(2).SleepUntil(() => _timeoutBroker.ReceivedCalls().Any());

            Subject.Stop();
        }

        [Test]
        public void SomethingShouldHappen()
        {
            _timeoutBroker.Received().Dispatch(Arg.Any<SomeTimeout>());
        }
    }
}