using System;
using NSubstitute;
using NUnit.Framework;
using Nimbus.Configuration;
using Nimbus.InfrastructureContracts;
using Nimbus.IntegrationTests.Tests.SimplePubSubTests.MessageContracts;

namespace Nimbus.IntegrationTests.Tests
{
    public abstract class SpecificationForBus : SpecificationFor<Bus>
    {
        protected ICommandBroker CommandBroker;
        protected IRequestBroker RequestBroker;
        protected IMulticastEventBroker MulticastEventBroker;
        protected ICompetingEventBroker CompetingEventBroker;

        public override Bus Given()
        {
            CommandBroker = Substitute.For<ICommandBroker>();
            RequestBroker = Substitute.For<IRequestBroker>();
            MulticastEventBroker = Substitute.For<IMulticastEventBroker>();
            CompetingEventBroker = Substitute.For<ICompetingEventBroker>();

            var typeProvider = new AssemblyScanningTypeProvider(typeof (SomeEventWeOnlyHandleViaMulticast).Assembly);

            var bus = new BusBuilder().Configure()
                                      .WithNames("MyTestSuite", Environment.MachineName)
                                      .WithConnectionString(CommonResources.ConnectionString)
                                      .WithTypesFrom(typeProvider)
                                      .WithCommandBroker(CommandBroker)
                                      .WithRequestBroker(RequestBroker)
                                      .WithMulticastEventBroker(MulticastEventBroker)
                                      .WithCompetingEventBroker(CompetingEventBroker)
                                      .WithDebugOptions(
                                          dc =>
                                          dc.RemoveAllExistingNamespaceElementsOnStartup(
                                              "I understand this will delete EVERYTHING in my namespace. I promise to only use this for test suites."))
                                      .Build();
            bus.Start();
            return bus;
        }

        [TearDown]
        public override void TearDown()
        {
            var bus = Subject;
            if (bus != null) bus.Stop();

            CommandBroker = null;
            RequestBroker = null;
            MulticastEventBroker = null;
            CompetingEventBroker = null;

            base.TearDown();
        }
    }
}