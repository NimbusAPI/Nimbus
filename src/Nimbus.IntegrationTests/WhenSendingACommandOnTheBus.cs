using System.Threading;
using NSubstitute;

namespace Nimbus.IntegrationTests
{
    public class WhenSendingACommandOnTheBus : SpecificationFor<Bus>
    {
        private IEventBroker _eventBroker;

        public override Bus Given()
        {

            var connectionString =
                @"Endpoint=sb://bazaario.servicebus.windows.net;SharedAccessKeyName=Main;SharedAccessKey=tTELgEQD4v7XvHpgkNDXwETKX4izhUhIoPTCtj/zOO8=;TransportType=Amqp";

            _eventBroker = Substitute.For<IEventBroker>();
            var bus = new Bus(connectionString, _eventBroker);
            bus.Start();
            return bus;
        }

        public override void When()
        {
            var someCommand = new SomeCommand();
            Subject.Send(someCommand);
            Thread.Sleep(2000);
        }

        [Then]
        public void SomethingShouldHappen()
        {
            _eventBroker.Received().Publish(Arg.Any<SomeCommand>());
        }
    }
}