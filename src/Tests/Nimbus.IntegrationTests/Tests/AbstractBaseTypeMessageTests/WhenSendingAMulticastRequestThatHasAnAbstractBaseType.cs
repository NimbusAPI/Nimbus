using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.IntegrationTests.Tests.AbstractBaseTypeMessageTests.MessageContracts;
using Nimbus.Tests.Common;
using Nimbus.Tests.Common.Extensions;
using Nimbus.Tests.Common.TestScenarioGeneration;
using Nimbus.Tests.Common.TestUtilities;
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
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingAMulticastRequestThatHasAnAbstractBaseType>))]
        public async Task TheHandlerShouldReceiveThatRequest(string testName, BusBuilderConfiguration busBuilderConfiguration)
        {
            await Given(busBuilderConfiguration);
            await When();

            MethodCallCounter.AllReceivedMessages.OfType<SomeConcreteRequestType>().Count().ShouldBe(1);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingAMulticastRequestThatHasAnAbstractBaseType>))]
        public async Task TheCorrectNumberOfTotalMessagesShouldHaveBeenObserved(string testName, BusBuilderConfiguration busBuilderConfiguration)
        {
            await Given(busBuilderConfiguration);
            await When();

            MethodCallCounter.AllReceivedMessages.Count().ShouldBe(1);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingAMulticastRequestThatHasAnAbstractBaseType>))]
        public async Task TheResponseShouldNotBeNull(string testName, BusBuilderConfiguration busBuilderConfiguration)
        {
            await Given(busBuilderConfiguration);
            await When();

            _response.ShouldNotBe(null);
        }
    }
}