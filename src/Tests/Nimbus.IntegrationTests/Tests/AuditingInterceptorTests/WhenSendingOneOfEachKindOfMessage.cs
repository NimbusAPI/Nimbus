using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Infrastructure.DependencyResolution;
using Nimbus.IntegrationTests.Tests.AuditingInterceptorTests.MessageTypes;
using Nimbus.Interceptors;
using Nimbus.MessageContracts.ControlMessages;
using Nimbus.Tests.Common.Extensions;
using Nimbus.Tests.Common.Stubs;
using Nimbus.Tests.Common.TestUtilities;
using Nimbus.Transports.InProcess;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.AuditingInterceptorTests
{
    [TestFixture]
    [Timeout(_timeoutSeconds*1000)]
    public class WhenSendingOneOfEachKindOfMessage : SpecificationForAsync<IBus>
    {
        private const int _timeoutSeconds = 15;

        protected override async Task<IBus> Given()
        {
            MethodCallCounter.Clear();

            var testFixtureType = GetType();
            var outboundAuditingInterceptorType = typeof (OutboundAuditingInterceptor);
            var auditEventType = typeof (AuditEvent);
            var typeProvider = new TestHarnessTypeProvider(new[] {testFixtureType.Assembly, outboundAuditingInterceptorType.Assembly},
                                                           new[] {testFixtureType.Namespace, outboundAuditingInterceptorType.Namespace, auditEventType.Namespace});
            var logger = TestHarnessLoggerFactory.Create();

            var dependencyResolver = new DependencyResolver(typeProvider);

            var bus = new BusBuilder().Configure()
                                      .WithTransport(new InProcessTransportConfiguration())
                                      .WithNames("MyTestSuite", Environment.MachineName)
                                      .WithTypesFrom(typeProvider)
                                      .WithDependencyResolver(dependencyResolver)
                                      .WithDefaultTimeout(TimeSpan.FromSeconds(10))
                                      .WithMaxDeliveryAttempts(1)
                                      .WithHeartbeatInterval(TimeSpan.MaxValue)
                                      .WithGlobalOutboundInterceptorTypes(typeof (OutboundAuditingInterceptor))
                                      .WithLogger(logger)
                                      .WithDebugOptions(
                                          dc =>
                                              dc.RemoveAllExistingNamespaceElementsOnStartup(
                                                  "I understand this will delete EVERYTHING in my namespace. I promise to only use this for test suites."))
                                      .Build();

            await bus.Start();

            return bus;
        }

        protected override async Task When()
        {
            await Task.WhenAll(
                Subject.Send(new SomeCommand(42)),
                Subject.SendAt(new SomeCommandSentViaDelay(), DateTimeOffset.UtcNow),
                Subject.Request(new SomeRequest()),
                Subject.MulticastRequest(new SomeMulticastRequest(), TimeSpan.FromSeconds(1)),
                Subject.Publish(new SomeEvent())
                );
        }

        private static object[] AllAuditedMessages()
        {
            return MethodCallCounter.AllReceivedMessages
                                    .OfType<AuditEvent>()
                                    .Select(ae => ae.MessageBody)
                                    .ToArray();
        }

        [Test]
        public async Task ThereShouldBeAnAuditRecordForSomeCommand()
        {
            await TimeSpan.FromSeconds(_timeoutSeconds).WaitUntil(() => AllAuditedMessages().OfType<SomeCommand>().Count() == 1);
            AllAuditedMessages().OfType<SomeCommand>().Count().ShouldBe(1);
        }

        [Test]
        public async Task ThereShouldBeAnAuditRecordForSomeCommandSentViaDelay()
        {
            await TimeSpan.FromSeconds(_timeoutSeconds).WaitUntil(() => AllAuditedMessages().OfType<SomeCommandSentViaDelay>().Count() == 1);
            AllAuditedMessages().OfType<SomeCommandSentViaDelay>().Count().ShouldBe(1);
        }

        [Test]
        public async Task ThereShouldBeAnAuditRecordForSomeRequest()
        {
            await TimeSpan.FromSeconds(_timeoutSeconds).WaitUntil(() => AllAuditedMessages().OfType<SomeRequest>().Count() == 1);
            AllAuditedMessages().OfType<SomeRequest>().Count().ShouldBe(1);
        }

        [Test]
        public async Task ThereShouldBeAnAuditRecordForSomeResponse()
        {
            await TimeSpan.FromSeconds(_timeoutSeconds).WaitUntil(() => AllAuditedMessages().OfType<SomeResponse>().Count() == 1);
            AllAuditedMessages().OfType<SomeResponse>().Count().ShouldBe(1);
        }

        [Test]
        public async Task ThereShouldBeAnAuditRecordForSomeMulticastRequest()
        {
            await TimeSpan.FromSeconds(_timeoutSeconds).WaitUntil(() => AllAuditedMessages().OfType<SomeMulticastRequest>().Count() == 1);
            AllAuditedMessages().OfType<SomeMulticastRequest>().Count().ShouldBe(1);
        }

        [Test]
        public async Task ThereShouldBeAnAuditRecordForSomeMulticastResponse()
        {
            await TimeSpan.FromSeconds(_timeoutSeconds).WaitUntil(() => AllAuditedMessages().OfType<SomeMulticastResponse>().Count() == 1);
            AllAuditedMessages().OfType<SomeMulticastResponse>().Count().ShouldBe(1);
        }

        [Test]
        public async Task ThereShouldBeAnAuditRecordForSomeEvent()
        {
            await TimeSpan.FromSeconds(_timeoutSeconds).WaitUntil(() => AllAuditedMessages().OfType<SomeEvent>().Count() == 1);
            AllAuditedMessages().OfType<SomeEvent>().Count().ShouldBe(1);
        }

        [Test]
        public async Task ThereShouldBeATotalOfSevenAuditRecords()
        {
            await TimeSpan.FromSeconds(_timeoutSeconds).WaitUntil(() => MethodCallCounter.AllReceivedMessages.Count() >= 7);
            MethodCallCounter.AllReceivedMessages.OfType<AuditEvent>().Count().ShouldBe(7);
        }

        [Test]
        public async Task ThereShouldBeATotalOfSevenRecordedHandlerCalls()
        {
            await TimeSpan.FromSeconds(_timeoutSeconds).WaitUntil(() => MethodCallCounter.AllReceivedMessages.Count() >= 7);
            MethodCallCounter.AllReceivedCalls.Count().ShouldBe(7);
        }
    }
}