﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.IntegrationTests.Tests.AbstractBaseTypeMessageTests.MessageContracts;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;
using Nimbus.Tests.Common.TestScenarioGeneration.TestCaseSources;
using Nimbus.Tests.Common.TestUtilities;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.AbstractBaseTypeMessageTests
{
    public class WhenSendingARequestThatHasAnAbstractBaseType : TestForBus
    {
        private SomeConcreteResponseType _response;

        protected override async Task When()
        {
            var request = new SomeConcreteRequestType();
            _response = await Bus.Request(request, TimeSpan.FromSeconds(TimeoutSeconds));
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingARequestThatHasAnAbstractBaseType>))]
        public async Task TheHandlerShouldReceiveThatRequest(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();

            MethodCallCounter.AllReceivedMessages.OfType<SomeConcreteRequestType>().Count().ShouldBe(1);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingARequestThatHasAnAbstractBaseType>))]
        public async Task TheCorrectNumberOfTotalMessagesShouldHaveBeenObserved(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();

            MethodCallCounter.AllReceivedMessages.Count().ShouldBe(1);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingARequestThatHasAnAbstractBaseType>))]
        public async Task TheResponseShouldNotBeNull(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();

            _response.ShouldNotBe(null);
        }
    }
}