using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.BrokeredMessageFactoryTests
{
    [TestFixture]
    internal class WhenCreatingASuccesfulResponseToARequestWithReplyToSessionIdNotSet : GivenABrokeredMessageFactory
    {
        private BrokeredMessage _request;
        private BrokeredMessage _response;

        protected override async Task When()
        {
            _request = new BrokeredMessage();
            _response = await Subject.CreateSuccessfulResponse(new TestResponse(), _request);
        }

        [Test]
        public void ThenTheResponseShouldNotHaveSessionIdSet()
        {
            _response.SessionId.ShouldBe(null);
        }

        [DataContract]
        public class TestResponse { }
    }
}
