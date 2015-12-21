using System;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.MessageContracts.Exceptions;
using Nimbus.Tests.Common.Stubs;
using Nimbus.Transports.WindowsServiceBus;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.BusBuilderTests
{
    [TestFixture]
    public class WhenStartingABusWithAnEndpointThatDoesNotExist
    {
        [Test]
        [Timeout(5*1000)]
        public async Task ItShouldGoBangQuickly()
        {
            var typeProvider = new TestHarnessTypeProvider(new[] {GetType().Assembly}, new[] {GetType().Namespace});

            var logger = TestHarnessLoggerFactory.Create();

            var bus = new BusBuilder().Configure()
                                      .WithDefaults(typeProvider)
                                      .WithTransport(new WindowsServiceBusTransportConfiguration()
                                                         .WithConnectionString(
                                                             @"Endpoint=sb://shouldnotexist.example.com/;SharedAccessKeyName=IntegrationTestHarness;SharedAccessKey=borkborkbork=")
                )
                                      .WithNames("IntegrationTestHarness", Environment.MachineName)
                                      .WithDefaultTimeout(TimeSpan.FromSeconds(2))
                                      .WithLogger(logger)
                                      .Build();

            try
            {
                await bus.Start();
                Assert.Fail();
            }
            catch (Exception e)
            {
                e.ShouldBeTypeOf<BusException>();
            }
        }
    }
}