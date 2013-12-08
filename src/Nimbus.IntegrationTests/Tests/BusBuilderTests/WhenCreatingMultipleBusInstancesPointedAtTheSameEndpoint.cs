using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Extensions;
using Nimbus.Logger;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.BusBuilderTests
{
    [TestFixture]
    public class WhenCreatingMultipleBusInstancesPointedAtTheSameEndpoint
    {
        [Test]
        public void NoneOfThemShouldGoBang()
        {
            ClearMeABus();

            var tasks = new List<Task>();
            for (var i = 0; i < 20; i++)
            {
                tasks.Add(Task.Run(() => BuildMeABus()));
            }
            tasks.WaitAll();
        }

        private void ClearMeABus()
        {
            // Filter types we care about to only our own test's namespace. It's a performance optimisation because creating and
            // deleting queues and topics is slow.
            var typeProvider = new TestHarnessTypeProvider(new[] {GetType().Assembly}, new[] {"Some.Namespace.That.Does.Not.Exist"});
            var messageBroker = new TestHarnessMessageBroker(typeProvider);

            var logger = new ConsoleLogger();

            var bus = new BusBuilder().Configure()
                                      .WithNames("MyTestSuite", Environment.MachineName)
                                      .WithConnectionString(CommonResources.ConnectionString)
                                      .WithTypesFrom(typeProvider)
                                      .WithCommandBroker(messageBroker)
                                      .WithRequestBroker(messageBroker)
                                      .WithMulticastEventBroker(messageBroker)
                                      .WithCompetingEventBroker(messageBroker)
                                      .WithMulticastRequestBroker(messageBroker)
                                      .WithDefaultTimeout(TimeSpan.FromSeconds(10))
                                      .WithLogger(logger)
                                      .WithDebugOptions(
                                          dc =>
                                              dc.RemoveAllExistingNamespaceElementsOnStartup(
                                                  "I understand this will delete EVERYTHING in my namespace. I promise to only use this for test suites."))
                                      .Build();
        }

        private void BuildMeABus()
        {
            // Filter types we care about to only our own test's namespace. It's a performance optimisation because creating and
            // deleting queues and topics is slow.
            var typeProvider = new TestHarnessTypeProvider(new[] {GetType().Assembly}, new[] {GetType().Namespace});
            var messageBroker = new TestHarnessMessageBroker(typeProvider);

            var logger = new ConsoleLogger();

            var bus = new BusBuilder().Configure()
                                      .WithNames("MyTestSuite", Environment.MachineName)
                                      .WithConnectionString(CommonResources.ConnectionString)
                                      .WithTypesFrom(typeProvider)
                                      .WithCommandBroker(messageBroker)
                                      .WithRequestBroker(messageBroker)
                                      .WithMulticastEventBroker(messageBroker)
                                      .WithCompetingEventBroker(messageBroker)
                                      .WithMulticastRequestBroker(messageBroker)
                                      .WithDefaultTimeout(TimeSpan.FromSeconds(10))
                                      .WithLogger(logger)
                                      .Build();
            bus.ShouldNotBe(null);
        }
    }
}