using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.BrokeredMessageFactoryTests
{
    [TestFixture]
    internal class WhenCreatingASuccesfulResponseToARequestWithReplyToSessionIdSet : GivenABrokeredMessageFactory
    {
        private BrokeredMessage _request;
        private BrokeredMessage _response;
        private string _sessionId;

        protected override async Task When()
        {
            _request = new BrokeredMessage();
            _sessionId = Guid.NewGuid().ToString();
            _request.ReplyToSessionId = _sessionId;
            _response = await Subject.CreateSuccessfulResponse(new TestResponse(), _request);
        }

        [Test]
        public void ThenTheResponseShouldUseThatSessionId()
        {
            _response.SessionId.ShouldBe(_sessionId);
        }

        [DataContract]
        public class TestResponse { }
    }
}
