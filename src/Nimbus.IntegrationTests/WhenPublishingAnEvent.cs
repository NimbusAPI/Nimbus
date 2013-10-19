using System.Threading;
using NSubstitute;

namespace Nimbus.IntegrationTests
{
    public class WhenPublishingAnEvent : SpecificationFor<Bus>
    {
        private ICommandBroker _commandBroker;
        private IRequestBroker _requestBroker;
        private IEventBroker _eventBroker;

        public override Bus Given()
        {
            _commandBroker = Substitute.For<ICommandBroker>();
            _requestBroker = Substitute.For<IRequestBroker>();
            _eventBroker = Substitute.For<IEventBroker>();

            var bus = new BusBuilder(CommonResources.ConnectionString,
                                     _commandBroker,
                                     _requestBroker,
                                     _eventBroker,
                                     new[] {typeof (SomeCommand)},
                                     new[] {typeof (SomeRequest)},
                                     new[] {typeof (SomeEvent)})
                .Build();
            bus.Start();
            return bus;
        }

        public override void When()
        {
            var myEvent = new SomeEvent();
            Subject.Publish(myEvent).Wait();

            Thread.Sleep(2000);

            Subject.Stop();
        }

        [Then]
        public void SomethingShouldHappen()
        {
            _eventBroker.Received().Publish(Arg.Any<SomeEvent>());
        }
    }
}