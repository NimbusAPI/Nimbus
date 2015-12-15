using System;
using System.Linq;
using System.Threading.Tasks;
using ConfigInjector.QuickAndDirty;
using Nimbus.Configuration;
using Nimbus.IntegrationTests.Configuration;
using Nimbus.IntegrationTests.Tests.LargeMessageTests.Handlers;
using Nimbus.IntegrationTests.Tests.LargeMessageTests.MessageContracts;
using Nimbus.LargeMessages.Azure.Configuration;
using Nimbus.Tests.Common;
using Nimbus.Transports.WindowsServiceBus;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.LargeMessageTests
{
    [TestFixture]
    [Timeout(30*1000)]
    public class WhenSendingALargeRequestUsingAzureBlobStorage : SpecificationForAsync<Bus>
    {
        private BigFatResponse _response;
        private BigFatRequest _busRequest;

        protected override async Task<Bus> Given()
        {
            var typeProvider = new TestHarnessTypeProvider(new[] {GetType().Assembly}, new[] {GetType().Namespace});
            var logger = TestHarnessLoggerFactory.Create();
            var largeMessageBodyStorage = new BlobStorageBuilder().Configure()
                                                                  .UsingStorageAccountConnectionString(DefaultSettingsReader.Get<BlobStorageConnectionString>())
                                                                  .WithLogger(logger)
                                                                  .Build();
            var bus = new BusBuilder().Configure()
                                      .WithTransport(new WindowsServiceBusTransportConfiguration())
                                      .WithNames("MyTestSuite", Environment.MachineName)
                                      .WithConnectionString(DefaultSettingsReader.Get<AzureServiceBusConnectionString>())
                                      .WithTypesFrom(typeProvider)
                                      .WithDefaultTimeout(TimeSpan.FromSeconds(10))
                                      .WithLogger(logger)
                                      .WithLargeMessageStorage(c => c.WithLargeMessageBodyStore(largeMessageBodyStorage)
                                                                     .WithMaxSmallMessageSize(64*1024)
                                                                     .WithMaxLargeMessageSize(10*1048576))
                                      .WithDebugOptions(dc => dc.RemoveAllExistingNamespaceElementsOnStartup(
                                          "I understand this will delete EVERYTHING in my namespace. I promise to only use this for test suites."))
                                      .Build();
            await bus.Start();
            return bus;
        }

        protected override async Task When()
        {
            var bigQuestion = new string(Enumerable.Range(0, BigFatRequestHandler.MessageSize).Select(i => '.').ToArray());

            _busRequest = new BigFatRequest
                          {
                              SomeBigQuestion = bigQuestion
                          };
            _response = await Subject.Request(_busRequest, TimeSpan.FromSeconds(60));
        }

        [Test]
        public async Task TheResponseShouldReturnUnscathed()
        {
            _response.SomeBigAnswer.Length.ShouldBe(BigFatRequestHandler.MessageSize);
        }
    }
}