using System;
using System.Threading;
using NSubstitute;

namespace Nimbus.IntegrationTests
{
    public class WhenSendingACommandOnTheBus : SpecificationFor<Bus>
    {
        private ICommandBroker _commandBroker;
        private IRequestBroker _requestBroker;
        private IEventBroker _eventBroker;

        public override Bus Given()
        {
            var connectionString = CommonResources.ConnectionString;

            _commandBroker = Substitute.For<ICommandBroker>();
            _requestBroker = Substitute.For<IRequestBroker>();
            _eventBroker = Substitute.For<IEventBroker>();

            var queueManager = new QueueManager(connectionString);

            var bus = new Bus(connectionString, queueManager, _commandBroker, _requestBroker, _eventBroker, new[] { typeof(SomeCommand) }, new[] { typeof(SomeRequest) }, new[] { typeof(SomeEvent) });
            bus.Start();
            return bus;
        }

        public override void When()
        {
            var someCommand = new SomeCommand();
            Subject.Send(someCommand);
            Thread.Sleep(TimeSpan.FromSeconds(2));

            Subject.Stop();
        }

        [Then]
        public void SomethingShouldHappen()
        {
            _commandBroker.Received().Dispatch(Arg.Any<SomeCommand>());
        }
    }
}