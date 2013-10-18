using NSubstitute;
using Shouldly;

namespace Nimbus.IntegrationTests
{
    public class WhenSendingARequestOnTheBus : SpecificationFor<Bus>
    {
        private IEventBroker _eventBroker;
        private SomeResponse _response;

        public override Bus Given()
        {
            var connectionString =
                @"Endpoint=sb://bazaario.servicebus.windows.net;SharedAccessKeyName=Main;SharedAccessKey=tTELgEQD4v7XvHpgkNDXwETKX4izhUhIoPTCtj/zOO8=;TransportType=Amqp";

            _eventBroker = Substitute.For<IEventBroker>();

            var bus = new Bus(connectionString, _eventBroker, new[] {typeof (SomeCommand)});
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