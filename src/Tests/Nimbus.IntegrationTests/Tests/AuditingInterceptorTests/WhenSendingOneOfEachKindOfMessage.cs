using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.DependencyResolution;
using Nimbus.IntegrationTests.Extensions;
using Nimbus.IntegrationTests.Tests.AuditingInterceptorTests.MessageTypes;
using Nimbus.Interceptors;
using Nimbus.Logger;
using Nimbus.MessageContracts.ControlMessages;
using Nimbus.Tests.Common;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.AuditingInterceptorTests
{
    [TestFixture]
    public class WhenSendingOneOfEachKindOfMessage : SpecificationForAsync<IBus>
    {
        private object[] _allAuditedMessages;

        protected override async Task<IBus> Given()
        {
            MethodCallCounter.Clear();

            var testFixtureType = GetType();
            var outboundAuditingInterceptorType = typeof (OutboundAuditingInterceptor);
            var auditEventType = typeof (AuditEvent);
            var typeProvider = new TestHarnessTypeProvider(new[] {testFixtureType.Assembly, outboundAuditingInterceptorType.Assembly},
                                                           new[] {testFixtureType.Namespace, outboundAuditingInterceptorType.Namespace, auditEventType.Namespace});
            var logger = new ConsoleLogger();

            var dependencyResolver = new DependencyResolver(typeProvider);

            var bus = new BusBuilder().Configure()
                                      .WithNames("MyTestSuite", Environment.MachineName)
                                      .WithConnectionString(CommonResources.ServiceBusConnectionString)
                                      .WithTypesFrom(typeProvider)
                                      .WithDependencyResolver(dependencyResolver)
                                      .WithDefaultTimeout(TimeSpan.FromSeconds(10))
                                      .WithMaxDeliveryAttempts(1)
                                      .WithGlobalOutboundInterceptorTypes(typeof (OutboundAuditingInterceptor))
                                      .WithLogger(logger)
                                      .WithDebugOptions(
                                          dc =>
                                          dc.RemoveAllExistingNamespaceElementsOnStartup(
                                              "I understand this will delete EVERYTHING in my namespace. I promise to only use this for test suites."))
                                      .Build();

            dependencyResolver.Register(bus, typeof (IBus));
            dependencyResolver.Register(new SystemClock(), typeof (IClock));

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

            TimeSpan.FromSeconds(2).SleepUntil(() => MethodCallCounter.AllReceivedMessages.Count() >= 7);

            _allAuditedMessages = MethodCallCounter.AllReceivedMessages
                                                   .OfType<AuditEvent>()
                                                   .Select(ae => ae.MessageBody)
                                                   .ToArray();
        }

        [Test]
        public void ThereShouldBeAnAuditRecordForSomeCommand()
        {
            _allAuditedMessages.OfType<SomeCommand>().Count().ShouldBe(1);
        }

        [Test]
        public void ThereShouldBeAnAuditRecordForSomeCommandSentViaDelay()
        {
            _allAuditedMessages.OfType<SomeCommandSentViaDelay>().Count().ShouldBe(1);
        }

        [Test]
        public void ThereShouldBeAnAuditRecordForSomeRequest()
        {
            _allAuditedMessages.OfType<SomeRequest>().Count().ShouldBe(1);
        }

        [Test]
        public void ThereShouldBeAnAuditRecordForSomeResponse()
        {
            _allAuditedMessages.OfType<SomeResponse>().Count().ShouldBe(1);
        }

        [Test]
        public void ThereShouldBeAnAuditRecordForSomeMulticastRequest()
        {
            _allAuditedMessages.OfType<SomeMulticastRequest>().Count().ShouldBe(1);
        }

        [Test]
        public void ThereShouldBeAnAuditRecordForSomeMulticastResponse()
        {
            _allAuditedMessages.OfType<SomeMulticastResponse>().Count().ShouldBe(1);
        }

        [Test]
        public void ThereShouldBeAnAuditRecordForSomeEvent()
        {
            _allAuditedMessages.OfType<SomeEvent>().Count().ShouldBe(1);
        }

        [Test]
        public void ThereShouldBeATotalOfSevenAuditRecords()
        {
            MethodCallCounter.AllReceivedMessages.OfType<AuditEvent>().Count().ShouldBe(7);
        }

        [Test]
        public void ThereShouldBeATotalOfSevenRecordedMethodCalls()
        {
            MethodCallCounter.AllReceivedCalls.Count().ShouldBe(7);
        }
    }
}