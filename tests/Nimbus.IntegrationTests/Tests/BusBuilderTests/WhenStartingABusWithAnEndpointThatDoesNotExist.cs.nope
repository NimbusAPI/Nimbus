using System;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.MessageContracts.Exceptions;
using Nimbus.Tests.Common.Stubs;
using Nimbus.Transports.AzureServiceBus;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.BusBuilderTests
{
    [TestFixture]
    [Parallelizable(ParallelScope.None)]
    public class WhenStartingABusWithAnEndpointThatDoesNotExist
    {
        protected const int TimeoutSeconds = 10;

        [Test]
        [Timeout(TimeoutSeconds*1000)]
        public async Task ItShouldGoBangQuickly()
        {
            var typeProvider = new TestHarnessTypeProvider(new[] {GetType().Assembly}, new[] {GetType().Namespace});
            var logger = TestHarnessLoggerFactory.Create(Guid.NewGuid(), GetType().FullName);

            using (
                var bus = new BusBuilder().Configure()
                                          .WithDefaults(typeProvider)
                                          .WithTransport(new AzureServiceBusTransportConfiguration()
                                                             .WithConnectionString(
                                                                 @"Endpoint=sb://shouldnotexist.example.com/;SharedAccessKeyName=IntegrationTestHarness;SharedAccessKey=borkborkbork=")
                    )
                                          .WithNames("IntegrationTestHarness", Environment.MachineName)
                                          .WithDefaultTimeout(TimeSpan.FromSeconds(TimeoutSeconds >> 1))
                                          .WithLogger(logger)
                                          .Build())
            {
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
}