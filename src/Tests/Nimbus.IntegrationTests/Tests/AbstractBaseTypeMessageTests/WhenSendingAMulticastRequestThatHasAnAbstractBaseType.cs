using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.IntegrationTests.Extensions;
using Nimbus.IntegrationTests.Tests.AbstractBaseTypeMessageTests.MessageContracts;
using Nimbus.Tests.Common;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.AbstractBaseTypeMessageTests
{
    public class WhenSendingAMulticastRequestThatHasAnAbstractBaseType : TestForBus
    {
        private SomeConcreteResponseType[] _response;

        protected override async Task When()
        {
            var request = new SomeConcreteRequestType();
            _response = (await Bus.MulticastRequest(request, TimeSpan.FromSeconds(2))).ToArray();

            await TimeSpan.FromSeconds(5).WaitUntil(() => MethodCallCounter.AllReceivedMessages.Any());
        }

        [Test]
        public async Task TheHandlerShouldReceiveThatRequest()
        {
            MethodCallCounter.AllReceivedMessages.OfType<SomeConcreteRequestType>().Count().ShouldBe(1);
        }

        [Test]
        public async Task TheCorrectNumberOfTotalMessagesShouldHaveBeenObserved()
        {
            MethodCallCounter.AllReceivedMessages.Count().ShouldBe(1);
        }

        [Test]
        public async Task TheResponseShouldNotBeNull()
        {
            _response.ShouldNotBe(null);
        }
    }
}