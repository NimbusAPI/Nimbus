using System;
using NSubstitute;
using Shouldly;

namespace Nimbus.IntegrationTests
{
    public class WhenSendingARequestOnTheBus : SpecificationFor<Bus>
    {
        public class FakeBroker : IRequestBroker
        {
            public bool DidGetCalled;

            public TBusResponse Handle<TBusRequest, TBusResponse>(TBusRequest request) where TBusRequest : BusRequest<TBusRequest, TBusResponse>
            {
                DidGetCalled = true;

                return Activator.CreateInstance<TBusResponse>();
            }
        }

        private ICommandBroker _commandBroker;
        private IRequestBroker _requestBroker;
        private IEventBroker _eventBroker;
        private SomeResponse _response;

        public override Bus Given()
        {
            var connectionString = CommonResources.ConnectionString;

            _commandBroker = Substitute.For<ICommandBroker>();
            _requestBroker = new FakeBroker();
            _eventBroker = Substitute.For<IEventBroker>();

            var queueManager = new QueueManager(connectionString);

            var bus = new Bus(connectionString, queueManager, _commandBroker, _requestBroker, _eventBroker, new[] {typeof (SomeCommand)}, new[] {typeof (SomeRequest)}, new[] {typeof (SomeEvent)});
            bus.Start();
            return bus;
        }

        public override void When()
        {
            var request = new SomeRequest();
            var task = Subject.Request(request);
            task.Wait(TimeSpan.FromSeconds(2));

            _response = task.Result;

            Subject.Stop();
        }

        [Then]
        public void WeShouldGetSomethingNiceBack()
        {
            _response.ShouldNotBe(null);
        }
    }
}