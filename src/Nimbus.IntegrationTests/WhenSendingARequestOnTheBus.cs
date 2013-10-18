using NSubstitute;
using Shouldly;

namespace Nimbus.IntegrationTests
{
    public class WhenSendingARequestOnTheBus : SpecificationFor<Bus>
    {
        private ICommandBroker _commandBroker;
        private IRequestBroker _requestBroker;
        private SomeResponse _response;

        public override Bus Given()
        {
            var connectionString =
                @"Endpoint=sb://bazaario.servicebus.windows.net;SharedAccessKeyName=ApplicationKey;SharedAccessKey=9+cooCqwistQKhrOQDUwCADCTLYFQc6q7qsWyZ8gxJo=;TransportType=Amqp";

            _commandBroker = Substitute.For<ICommandBroker>();
            _requestBroker = Substitute.For<IRequestBroker>();

            _requestBroker.Handle<SomeRequest, SomeResponse>(Arg.Any<SomeRequest>()).Returns(ci => new SomeResponse());

            var bus = new Bus(connectionString, _commandBroker, _requestBroker, new[] {typeof (SomeCommand)}, new [] {typeof(SomeRequest)});
            bus.Start();

            return bus;
        }

        public override  void When()
        {
            var request = new SomeRequest();
            var task = Subject.Request(request);
            task.Wait();

            _response = task.Result;
        }

        [Then]
        public void WeShouldGetSomethingNiceBack()
        {
            _response.ShouldNotBe(null);
        }
    }
}