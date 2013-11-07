using System;
using NSubstitute;
using Nimbus.Configuration;
using Nimbus.InfrastructureContracts;
using Nimbus.IntegrationTests.Tests.SimplePubSubTests.MessageContracts;

namespace Nimbus.IntegrationTests.Tests
{
    public abstract class SpecificationForBus : SpecificationFor<Bus>
    {
        protected ICommandBroker _commandBroker;
        protected IRequestBroker _requestBroker;
        protected IMulticastEventBroker _multicastEventBroker;
        protected ICompetingEventBroker _competingEventBroker;

        public override Bus Given()
        {
            _commandBroker = Substitute.For<ICommandBroker>();
            _requestBroker = Substitute.For<IRequestBroker>();
            _multicastEventBroker = Substitute.For<IMulticastEventBroker>();
            _competingEventBroker = Substitute.For<ICompetingEventBroker>();

            var typeProvider = new AssemblyScanningTypeProvider(typeof (SomeEventWeOnlyHandleViaMulticast).Assembly);

            var bus = new BusBuilder().Configure()
                                      .WithNames("MyTestSuite", Environment.MachineName)
                                      .WithConnectionString(CommonResources.ConnectionString)
                                      .WithTypesFrom(typeProvider)
                                      .WithCommandBroker(_commandBroker)
                                      .WithRequestBroker(_requestBroker)
                                      .WithMulticastEventBroker(_multicastEventBroker)
                                      .WithCompetingEventBroker(_competingEventBroker)
                                      .WithDebugOptions(
                                          dc =>
                                          dc.RemoveAllExistingNamespaceElementsOnStartup(
                                              "I understand this will delete EVERYTHING in my namespace. I promise to only use this for test suites."))
                                      .Build();
            bus.Start();
            return bus;
        }
    }
}