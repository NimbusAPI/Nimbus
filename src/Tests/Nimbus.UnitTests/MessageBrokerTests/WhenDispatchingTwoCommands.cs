using System.Reflection;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;
using Nimbus.UnitTests.MessageBrokerTests.Handlers;
using Nimbus.UnitTests.MessageBrokerTests.MessageContracts;
using NUnit.Framework;

namespace Nimbus.UnitTests.MessageBrokerTests
{
    [TestFixture]
    public class WhenDispatchingTwoCommands : SpecificationFor<ICommandBroker>
    {
        private BrokerTestCommand _command;

        public override ICommandBroker Given()
        {
            var typeProvider = new TestHarnessTypeProvider(new[] {Assembly.GetExecutingAssembly()}, new[] {GetType().Namespace});
            return new DefaultMessageBroker(typeProvider);
        }

        public override void When()
        {
            _command = new BrokerTestCommand();
            Subject.Dispatch(_command);
        }

        [Test]
        public void ItShouldBeDispatchedToTheCorrectHandler()
        {
            MethodCallCounter.ReceivedCallsWithAnyArg<BrokerTestCommandHandler>(h => h.Handle(_command));
        }
    }
}