using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Nimbus.Configuration;

namespace Nimbus.IntegrationTests
{
    [TestFixture]
    public abstract class SpecificationForBus : SpecificationFor<Bus>
    {
        protected TestHarnessMessageBroker MessageBroker { get; private set; }

        public override Bus Given()
        {
            // Filter types we care about to only our own test's namespace. It's a performance optimisation because creating and
            // deleting queues and topics is slow.
            var typeProvider = new TestHarnessTypeProvider(new[] {GetType().Assembly}, new[] {GetType().Namespace});
            MessageBroker = new TestHarnessMessageBroker(typeProvider);

            var bus = new BusBuilder().Configure()
                                      .WithNames("MyTestSuite", Environment.MachineName)
                                      .WithConnectionString(CommonResources.ConnectionString)
                                      .WithTypesFrom(typeProvider)
                                      .WithCommandBroker(MessageBroker)
                                      .WithRequestBroker(MessageBroker)
                                      .WithMulticastEventBroker(MessageBroker)
                                      .WithCompetingEventBroker(MessageBroker)
                                      .WithDefaultTimeout(TimeSpan.FromSeconds(10))
                                      .WithDebugOptions(
                                          dc =>
                                          dc.RemoveAllExistingNamespaceElementsOnStartup(
                                              "I understand this will delete EVERYTHING in my namespace. I promise to only use this for test suites."))
                                      .Build();
            bus.Start();
            return bus;
        }

        public override sealed void When()
        {
            WhenAsync().Wait();
        }

        public abstract Task WhenAsync();

        [TearDown]
        public override void TearDown()
        {
            var bus = Subject;
            if (bus != null) bus.Stop();

            MessageBroker = null;

            base.TearDown();
        }
    }
}