using System;
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
                @"Endpoint=sb://bazaario.servicebus.windows.net/;SharedAccessKeyName=ApplicationKey;SharedAccessKey=9+cooCqwistQKhrOQDUwCADCTLYFQc6q7qsWyZ8gxJo=;TransportType=Amqp";

            _eventBroker = Substitute.For<IEventBroker>();
            var bus = new Bus(connectionString, _eventBroker, new[] {typeof (SomeCommand)});
            bus.Start();
            return bus;
        }

        public override void When()
        {
            var someCommand = new SomeCommand();
            Subject.Send(someCommand);
            Thread.Sleep(TimeSpan.FromSeconds(1));
        }

        [Then]
        public void SomethingShouldHappen()
        {
            _eventBroker.Received().Publish(Arg.Any<SomeCommand>());
        }
    }
}