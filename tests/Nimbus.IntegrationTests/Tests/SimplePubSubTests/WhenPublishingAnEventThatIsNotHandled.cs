﻿using System;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.IntegrationTests.Tests.SimplePubSubTests.MessageContracts;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;
using Nimbus.Tests.Common.TestScenarioGeneration.TestCaseSources;
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
        [TestCaseSource(typeof (AllBusConfigurations<WhenPublishingAnEventThatIsNotHandled>))]
        public async Task Run(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();
            await Then();
        }

        [Then]
        public async Task NoExceptionIsThrown()
        {
            _exception.ShouldBe(null);
        }
    }
}