using System;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Infrastructure;
using Nimbus.Logger;
using Nimbus.MessageContracts.Exceptions;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.BusBuilderTests
{
    [TestFixture]
    public class WhenStartingABusWithAnEndpointThatDoesNotExist
    {
        [Test]
        [Timeout(10*1000)]
        public async Task ItShouldGoBangQuickly()
        {
            var typeProvider = new TestHarnessTypeProvider(new[] {GetType().Assembly}, new[] {GetType().Namespace});
            var messageBroker = new DefaultMessageHandlerFactory(typeProvider);

            var logger = new ConsoleLogger();

            var bus = new BusBuilder().Configure()
                                      .WithNames("IntegrationTestHarness", Environment.MachineName)
                                      .WithConnectionString(@"Endpoint=sb://shouldnotexist.example.com/;SharedAccessKeyName=IntegrationTestHarness;SharedAccessKey=borkborkbork=")
                                      .WithTypesFrom(typeProvider)
                                      .WithCommandHandlerFactory(messageBroker)
                                      .WithRequestBroker(messageBroker)
                                      .WithMulticastEventBroker(messageBroker)
                                      .WithCompetingEventHandlerFactory(messageBroker)
                                      .WithMulticastRequestBroker(messageBroker)
                                      .WithDefaultTimeout(TimeSpan.FromSeconds(10))
                                      .WithLogger(logger)
                                      .Build();

            Should.Throw<BusException>(bus.Start);
        }
    }
}