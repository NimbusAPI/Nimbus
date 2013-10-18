using System;
using System.Threading;
using NSubstitute;

namespace Nimbus.IntegrationTests
{
    public class WhenSendingACommandOnTheBus : SpecificationFor<Bus>
    {
        private ICommandBroker _commandBroker;
        private IRequestBroker _requestBroker;

        public override Bus Given()
        {
            var connectionString =
                @"Endpoint=sb://bazaario.servicebus.windows.net/;SharedAccessKeyName=ApplicationKey;SharedAccessKey=9+cooCqwistQKhrOQDUwCADCTLYFQc6q7qsWyZ8gxJo=;TransportType=Amqp";

            _commandBroker = Substitute.For<ICommandBroker>();
            _requestBroker = Substitute.For<IRequestBroker>();
            var bus = new Bus(connectionString, _commandBroker, _requestBroker, new[] {typeof (SomeCommand)}, new []{typeof(SomeRequest)});
            bus.Start();
            return bus;
        }

        public override void When()
        {
            var someCommand = new SomeCommand();
            Subject.Send(someCommand);
            Thread.Sleep(TimeSpan.FromSeconds(1));

            Subject.Stop();
        }

        [Then]
        public void SomethingShouldHappen()
        {
            _commandBroker.Received().Dispatch(Arg.Any<SomeCommand>());
        }
    }
}