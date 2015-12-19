using System;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.IntegrationTests.Tests.SimplePubSubTests.MessageContracts;
using Nimbus.IntegrationTests.TestScenarioGeneration;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.SimplePubSubTests
{
    public class WhenPublishingAnEventThatIsNotHandled : TestForBus
    {
        private Exception _exception;

        protected override async Task When()
        {
            try
            {
                var myEvent = new SomeEventWeDoNotHandle();
                await Bus.Publish(myEvent);
            }
            catch (Exception exc)
            {
                _exception = exc;
            }
        }

        [Test]
        [TestCaseSource(typeof (TestForAllBusConfigurations<WhenPublishingAnEventThatIsNotHandled>))]
        public async Task NoExceptionIsThrown(string testName, BusBuilderConfiguration busBuilderConfiguration)
        {
            await Given(busBuilderConfiguration);
            await When();

            _exception.ShouldBe(null);
        }
    }
}