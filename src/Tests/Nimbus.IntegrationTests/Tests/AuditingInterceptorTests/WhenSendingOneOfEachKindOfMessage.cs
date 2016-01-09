using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.IntegrationTests.Tests.AuditingInterceptorTests.MessageTypes;
using Nimbus.Interceptors;
using Nimbus.MessageContracts.ControlMessages;
using Nimbus.Tests.Common.Extensions;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;
using Nimbus.Tests.Common.TestScenarioGeneration.TestCaseSources;
using Nimbus.Tests.Common.TestUtilities;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.AuditingInterceptorTests
{
    public class WhenSendingOneOfEachKindOfMessage : TestForBus
    {
        protected override void Reconfigure()
        {
            Instance.Configuration
                    .WithGlobalOutboundInterceptorTypes(typeof (OutboundAuditingInterceptor))
                    .WithMaxDeliveryAttempts(1)
                    .WithHeartbeatInterval(TimeSpan.MaxValue)
                ;
        }

        protected override async Task When()
        {
            await Task.WhenAll(
                Bus.Send(new SomeCommand(42)),
                Bus.SendAt(new SomeCommandSentViaDelay(), DateTimeOffset.UtcNow),
                Bus.Request(new SomeRequest()),
                Bus.MulticastRequest(new SomeMulticastRequest(), TimeSpan.FromSeconds(TimeoutSeconds)),
                Bus.Publish(new SomeEvent())
                );
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingOneOfEachKindOfMessage>))]
        public async Task ThereShouldBeAnAuditRecordForSomeCommand(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();

            await TimeSpan.FromSeconds(TimeoutSeconds).WaitUntil(() => AllAuditedMessages().OfType<SomeCommand>().Count() == 1);
            AllAuditedMessages().OfType<SomeCommand>().Count().ShouldBe(1);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingOneOfEachKindOfMessage>))]
        public async Task ThereShouldBeAnAuditRecordForSomeCommandSentViaDelay(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();

            await TimeSpan.FromSeconds(TimeoutSeconds).WaitUntil(() => AllAuditedMessages().OfType<SomeCommandSentViaDelay>().Count() == 1);
            AllAuditedMessages().OfType<SomeCommandSentViaDelay>().Count().ShouldBe(1);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingOneOfEachKindOfMessage>))]
        public async Task ThereShouldBeAnAuditRecordForSomeRequest(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();

            await TimeSpan.FromSeconds(TimeoutSeconds).WaitUntil(() => AllAuditedMessages().OfType<SomeRequest>().Count() == 1);
            AllAuditedMessages().OfType<SomeRequest>().Count().ShouldBe(1);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingOneOfEachKindOfMessage>))]
        public async Task ThereShouldBeAnAuditRecordForSomeResponse(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();

            await TimeSpan.FromSeconds(TimeoutSeconds).WaitUntil(() => AllAuditedMessages().OfType<SomeResponse>().Count() == 1);
            AllAuditedMessages().OfType<SomeResponse>().Count().ShouldBe(1);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingOneOfEachKindOfMessage>))]
        public async Task ThereShouldBeAnAuditRecordForSomeMulticastRequest(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();

            await TimeSpan.FromSeconds(TimeoutSeconds).WaitUntil(() => AllAuditedMessages().OfType<SomeMulticastRequest>().Count() == 1);
            AllAuditedMessages().OfType<SomeMulticastRequest>().Count().ShouldBe(1);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingOneOfEachKindOfMessage>))]
        public async Task ThereShouldBeAnAuditRecordForSomeMulticastResponse(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();

            await TimeSpan.FromSeconds(TimeoutSeconds).WaitUntil(() => AllAuditedMessages().OfType<SomeMulticastResponse>().Count() == 1);
            AllAuditedMessages().OfType<SomeMulticastResponse>().Count().ShouldBe(1);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingOneOfEachKindOfMessage>))]
        public async Task ThereShouldBeAnAuditRecordForSomeEvent(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();

            await TimeSpan.FromSeconds(TimeoutSeconds).WaitUntil(() => AllAuditedMessages().OfType<SomeEvent>().Count() == 1);
            AllAuditedMessages().OfType<SomeEvent>().Count().ShouldBe(1);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingOneOfEachKindOfMessage>))]
        public async Task ThereShouldBeATotalOfSevenAuditRecords(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();

            await TimeSpan.FromSeconds(TimeoutSeconds).WaitUntil(() => MethodCallCounter.AllReceivedMessages.Count() >= 7);
            MethodCallCounter.AllReceivedMessages.OfType<AuditEvent>().Count().ShouldBe(7);
        }

        [Test]
        [TestCaseSource(typeof (AllBusConfigurations<WhenSendingOneOfEachKindOfMessage>))]
        public async Task ThereShouldBeATotalOfSevenRecordedHandlerCalls(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();

            await TimeSpan.FromSeconds(TimeoutSeconds).WaitUntil(() => MethodCallCounter.AllReceivedMessages.Count() >= 7);
            MethodCallCounter.AllReceivedCalls.Count().ShouldBe(7);
        }

        private static object[] AllAuditedMessages()
        {
            return MethodCallCounter.AllReceivedMessages
                                    .OfType<AuditEvent>()
                                    .Select(ae => ae.MessageBody)
                                    .ToArray();
        }
    }
}