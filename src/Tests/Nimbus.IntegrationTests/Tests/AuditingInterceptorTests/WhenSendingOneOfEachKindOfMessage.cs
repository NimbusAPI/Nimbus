using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Infrastructure.DependencyResolution;
using Nimbus.IntegrationTests.Tests.AuditingInterceptorTests.MessageTypes;
using Nimbus.Interceptors;
using Nimbus.MessageContracts.ControlMessages;
using Nimbus.Tests.Common;
using Nimbus.Transports.InProcess;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.AuditingInterceptorTests
{
    [TestFixture]
    [Timeout(_timeoutSeconds*1000)]
    public class WhenSendingOneOfEachKindOfMessage : SpecificationForAsync<IBus>
    {
        private const int _timeoutSeconds = 5;

        private object[] _allAuditedMessages;

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

            await TimeSpan.FromSeconds(_timeoutSeconds).WaitUntil(() => MethodCallCounter.AllReceivedMessages.Count() >= 7);
            MethodCallCounter.Stop();

            _allAuditedMessages = MethodCallCounter.AllReceivedMessages
                                                   .OfType<AuditEvent>()
                                                   .Select(ae => ae.MessageBody)
                                                   .ToArray();
        }

        [Test]
        public async Task ThereShouldBeAnAuditRecordForSomeCommand()
        {
            _allAuditedMessages.OfType<SomeCommand>().Count().ShouldBe(1);
        }

        [Test]
        public async Task ThereShouldBeAnAuditRecordForSomeCommandSentViaDelay()
        {
            _allAuditedMessages.OfType<SomeCommandSentViaDelay>().Count().ShouldBe(1);
        }

        [Test]
        public async Task ThereShouldBeAnAuditRecordForSomeRequest()
        {
            _allAuditedMessages.OfType<SomeRequest>().Count().ShouldBe(1);
        }

        [Test]
        public async Task ThereShouldBeAnAuditRecordForSomeResponse()
        {
            _allAuditedMessages.OfType<SomeResponse>().Count().ShouldBe(1);
        }

        [Test]
        public async Task ThereShouldBeAnAuditRecordForSomeMulticastRequest()
        {
            _allAuditedMessages.OfType<SomeMulticastRequest>().Count().ShouldBe(1);
        }

        [Test]
        public async Task ThereShouldBeAnAuditRecordForSomeMulticastResponse()
        {
            _allAuditedMessages.OfType<SomeMulticastResponse>().Count().ShouldBe(1);
        }

        [Test]
        public async Task ThereShouldBeAnAuditRecordForSomeEvent()
        {
            _allAuditedMessages.OfType<SomeEvent>().Count().ShouldBe(1);
        }

        [Test]
        public async Task ThereShouldBeATotalOfSevenAuditRecords()
        {
            MethodCallCounter.AllReceivedMessages.OfType<AuditEvent>().Count().ShouldBe(7);
        }

        [Test]
        public async Task ThereShouldBeATotalOfSevenRecordedHandlerCalls()
        {
            MethodCallCounter.AllReceivedCalls.Count().ShouldBe(7);
        }
    }
}