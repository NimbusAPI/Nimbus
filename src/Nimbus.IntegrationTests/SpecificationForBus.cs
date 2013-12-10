using System.Threading.Tasks;
using Nimbus.Tests;
using NUnit.Framework;

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

            var messageBroker = new TestHarnessMessageBroker(typeProvider);
            MessageBroker = messageBroker;

            var bus = TestHarnessBusFactory.Create(typeProvider, messageBroker);
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