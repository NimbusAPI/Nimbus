using NSubstitute;

namespace Nimbus.IntegrationTests
{
    public class WhenSendingACommandOnTheBus : SpecificationFor<Bus>
    {
        private IEventBroker _eventBroker;

        public override Bus Given()
        {
            _eventBroker = Substitute.For<IEventBroker>();
            var bus = new Bus(_eventBroker);
            return bus;
        }

        public override void When()
        {
            var someCommand = new SomeCommand();
            Subject.Send(someCommand);
        }

        public void SomethingShouldHappen()
        {
            _eventBroker.Received().Publish(Arg.Any<SomeCommand>());
        }
    }
}