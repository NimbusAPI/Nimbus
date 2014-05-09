using System;
using System.Threading.Tasks;
using Nimbus.Configuration;
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
        [Timeout(15*1000)]
        public async Task ItShouldGoBangQuickly()
        {
            var typeProvider = new TestHarnessTypeProvider(new[] {GetType().Assembly}, new[] {GetType().Namespace});

            var logger = new ConsoleLogger();

            var bus = new BusBuilder().Configure()
                                      .WithNames("IntegrationTestHarness", Environment.MachineName)
                                      .WithConnectionString(@"Endpoint=sb://shouldnotexist.example.com/;SharedAccessKeyName=IntegrationTestHarness;SharedAccessKey=borkborkbork=")
                                      .WithTypesFrom(typeProvider)
                                      .WithDefaultTimeout(TimeSpan.FromSeconds(10))
                                      .WithLogger(logger)
                                      .Build();

            Should.Throw<BusException>(() => bus.Start());
        }
    }
}