using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Infrastructure.DependencyResolution;
using Nimbus.IntegrationTests.Extensions;
using Nimbus.IntegrationTests.Tests.AuditingInterceptorTests.MessageTypes;
using Nimbus.Interceptors;
using Nimbus.Logger;
using Nimbus.Tests.Common;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.AuditingInterceptorTests
{
    [TestFixture]
    public class WhenSendingOneOfEachKindOfMessage : SpecificationForAsync<IBus>
    {
        protected override async Task<IBus> Given()
        {
            MethodCallCounter.Clear();

            var testFixtureType = GetType();
            var outboundAuditingInterceptorType = typeof (OutboundAuditingInterceptor);
            var typeProvider = new TestHarnessTypeProvider(new[] {testFixtureType.Assembly, outboundAuditingInterceptorType.Assembly},
                                                           new[] {testFixtureType.Namespace, outboundAuditingInterceptorType.Namespace});
            var logger = new ConsoleLogger();

            var bus = new BusBuilder().Configure()
                                      .WithNames("MyTestSuite", Environment.MachineName)
                                      .WithConnectionString(CommonResources.ServiceBusConnectionString)
                                      .WithTypesFrom(typeProvider)
                                      .WithDependencyResolver(new DependencyResolver(typeProvider))
                                      .WithDefaultTimeout(TimeSpan.FromSeconds(10))
                                      .WithMaxDeliveryAttempts(1)
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
                Subject.Send(new SomeCommand()),
                Subject.SendAt(new SomeCommandSentViaDelay(), DateTimeOffset.UtcNow),
                Subject.Request(new SomeRequest()),
                Subject.MulticastRequest(new SomeMulticastRequest(), TimeSpan.FromSeconds(1)),
                Subject.Publish(new SomeEvent())
                );

            TimeSpan.FromSeconds(10).SleepUntil(() => MethodCallCounter.AllReceivedMessages.Count() >= 7);
        }

        [Test]
        public void ThereShouldBeAnAuditRecordForSomeCommand()
        {
            MethodCallCounter.AllReceivedMessages.OfType<SomeCommand>().Count().ShouldBe(1);
        }

        [Test]
        public void ThereShouldBeAnAuditRecordForSomeCommandSentViaDelay()
        {
            MethodCallCounter.AllReceivedMessages.OfType<SomeCommandSentViaDelay>().Count().ShouldBe(1);
        }

        [Test]
        public void ThereShouldBeAnAuditRecordForSomeRequest()
        {
            MethodCallCounter.AllReceivedMessages.OfType<SomeRequest>().Count().ShouldBe(1);
        }

        [Test]
        public void ThereShouldBeAnAuditRecordForSomeResponse()
        {
            MethodCallCounter.AllReceivedMessages.OfType<SomeResponse>().Count().ShouldBe(1);
        }

        [Test]
        public void ThereShouldBeAnAuditRecordForSomeMulticastRequest()
        {
            MethodCallCounter.AllReceivedMessages.OfType<SomeMulticastRequest>().Count().ShouldBe(1);
        }

        [Test]
        public void ThereShouldBeAnAuditRecordForSomeMulticastResponse()
        {
            MethodCallCounter.AllReceivedMessages.OfType<SomeMulticastResponse>().Count().ShouldBe(1);
        }

        [Test]
        public void ThereShouldBeAnAuditRecordForSomeEvent()
        {
            MethodCallCounter.AllReceivedMessages.OfType<SomeEvent>().Count().ShouldBe(1);
        }
    }
}