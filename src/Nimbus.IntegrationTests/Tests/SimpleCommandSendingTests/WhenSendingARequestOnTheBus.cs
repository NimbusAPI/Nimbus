using System;
using NUnit.Framework;
using Nimbus.InfrastructureContracts;
using Nimbus.IntegrationTests.Extensions;
using Nimbus.IntegrationTests.Tests.SimpleRequestResponseTests.MessageContracts;
using Nimbus.MessageContracts;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.SimpleCommandSendingTests
{
    [TestFixture]
    public class WhenSendingARequestOnTheBus : SpecificationForBus
    {
        public class FakeBroker : IRequestBroker
        {
            public bool DidGetCalled;

            public TBusResponse Handle<TBusRequest, TBusResponse>(TBusRequest request) where TBusRequest : BusRequest<TBusRequest, TBusResponse> where TBusResponse : IBusResponse
            {
                DidGetCalled = true;

                return Activator.CreateInstance<TBusResponse>();
            }
        }

        private SomeResponse _response;

        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(10);

        public override void When()
        {
            var request = new SomeRequest();
            var task = Subject.Request(request, _timeout);
            _response = task.WaitForResult(_timeout);

            Subject.Stop();
        }

        [Test]
        public void WeShouldGetSomethingNiceBack()
        {
            _response.ShouldNotBe(null);
        }
    }
}