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
                @"Endpoint=sb://bazaario.servicebus.windows.net;SharedAccessKeyName=Main;SharedAccessKey=tTELgEQD4v7XvHpgkNDXwETKX4izhUhIoPTCtj/zOO8=;TransportType=Amqp";

            _commandBroker = Substitute.For<ICommandBroker>();
            _requestBroker = Substitute.For<IRequestBroker>();

            _requestBroker.Handle<SomeRequest, SomeResponse>(Arg.Any<SomeRequest>()).Returns(new SomeResponse());

            var bus = new Bus(connectionString, _commandBroker, _requestBroker, new[] {typeof (SomeCommand)}, new [] {typeof(SomeRequest)});
            bus.Start();

            return bus;
        }

        public override async void When()
        {
            var request = new SomeRequest();
            _response = await Subject.Request(request);
        }

        [Then]
        public void WeShouldGetSomethingNiceBack()
        {
            _response.ShouldNotBe(null);
        }
    }
}