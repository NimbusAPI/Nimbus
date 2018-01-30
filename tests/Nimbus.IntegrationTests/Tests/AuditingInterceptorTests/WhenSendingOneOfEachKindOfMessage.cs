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
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.AuditingInterceptorTests
{
    public class WhenSendingOneOfEachKindOfMessage : TestForBus
    {
        private SomeResponse _response;
        private SomeMulticastResponse[] _multicastResponse;

        protected override void Reconfigure()
        {
            Instance.Configuration
                    .WithGlobalOutboundInterceptorTypes(typeof(OutboundAuditingInterceptor))
                    .WithMaxDeliveryAttempts(1)
                    .WithHeartbeatInterval(TimeSpan.MaxValue)
                ;
        }

        protected override async Task When()
        {
            await Task.WhenAll(
                Bus.Send(new SomeCommand(42)),
                Bus.SendAt(new SomeCommandSentViaDelay(), DateTimeOffset.UtcNow),
                Bus.Publish(new SomeEvent())
                );

            _response = await Bus.Request(new SomeRequest());

            // take just one so that we don't wait until the test times out.
            _multicastResponse = (await Bus.MulticastRequest(new SomeMulticastRequest(), TimeSpan.FromSeconds(TimeoutSeconds)))
                .Take(1)
                .ToArray();
        }

        [Test]
        [TestCaseSource(typeof(AllBusConfigurations<WhenSendingOneOfEachKindOfMessage>))]
        public async Task Run(string testName, IConfigurationScenario<BusBuilderConfiguration> scenario)
        {
            await Given(scenario);
            await When();
            await Then();
        }

        [Then]
        public async Task TheRequestShouldHaveReceivedAResponse()
        {
            _response.ShouldBeTypeOf<SomeResponse>();
        }

        [Then]
        public async Task TheMulticastRequestShouldHaveReceivedAResponse()
        {
            _multicastResponse.Length.ShouldBe(1);
        }

        [Then]
        public async Task ThereShouldBeAnAuditRecordForSomeCommand()
        {
            await Timeout.WaitUntil(() => AllAuditedMessages().OfType<SomeCommand>().Count() == 1);
            AllAuditedMessages().OfType<SomeCommand>().Count().ShouldBe(1);
        }

        [Then]
        public async Task ThereShouldBeAnAuditRecordForSomeCommandSentViaDelay()
        {
            await Timeout.WaitUntil(() => AllAuditedMessages().OfType<SomeCommandSentViaDelay>().Count() == 1);
            AllAuditedMessages().OfType<SomeCommandSentViaDelay>().Count().ShouldBe(1);
        }

        [Then]
        public async Task ThereShouldBeAnAuditRecordForSomeRequest()
        {
            await Timeout.WaitUntil(() => AllAuditedMessages().OfType<SomeRequest>().Count() == 1);
            AllAuditedMessages().OfType<SomeRequest>().Count().ShouldBe(1);
        }

        [Then]
        public async Task ThereShouldBeAnAuditRecordForSomeResponse()
        {
            await Timeout.WaitUntil(() => AllAuditedMessages().OfType<SomeResponse>().Count() == 1);
            AllAuditedMessages().OfType<SomeResponse>().Count().ShouldBe(1);
        }

        [Then]
        public async Task ThereShouldBeAnAuditRecordForSomeMulticastRequest()
        {
            await Timeout.WaitUntil(() => AllAuditedMessages().OfType<SomeMulticastRequest>().Count() == 1);
            AllAuditedMessages().OfType<SomeMulticastRequest>().Count().ShouldBe(1);
        }

        [Then]
        public async Task ThereShouldBeAnAuditRecordForSomeMulticastResponse()
        {
            await Timeout.WaitUntil(() => AllAuditedMessages().OfType<SomeMulticastResponse>().Count() == 1);
            AllAuditedMessages().OfType<SomeMulticastResponse>().Count().ShouldBe(1);
        }

        [Then]
        public async Task ThereShouldBeAnAuditRecordForSomeEvent()
        {
            await Timeout.WaitUntil(() => AllAuditedMessages().OfType<SomeEvent>().Count() == 1);
            AllAuditedMessages().OfType<SomeEvent>().Count().ShouldBe(1);
        }

        [Then]
        public async Task ThereShouldBeATotalOfSevenAuditRecords()
        {
            await Timeout.WaitUntil(() => MethodCallCounter.AllReceivedMessages.Count() >= 7);
            MethodCallCounter.AllReceivedMessages.OfType<AuditEvent>().Count().ShouldBe(7);
        }

        [Then]
        public async Task ThereShouldBeATotalOfSevenRecordedHandlerCalls()
        {
            await Timeout.WaitUntil(() => MethodCallCounter.AllReceivedMessages.Count() >= 7);
            MethodCallCounter.AllReceivedCalls.Count().ShouldBe(7);
        }

        private object[] AllAuditedMessages()
        {
            return MethodCallCounter.AllReceivedMessages
                                    .OfType<AuditEvent>()
                                    .Select(ae => ae.MessageBody)
                                    .ToArray();
        }
    }
}