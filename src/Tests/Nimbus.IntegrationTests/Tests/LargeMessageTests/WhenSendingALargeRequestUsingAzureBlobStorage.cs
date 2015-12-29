using System;
using System.Linq;
using System.Threading.Tasks;
using ConfigInjector.QuickAndDirty;
using Nimbus.Configuration;
using Nimbus.Infrastructure.DependencyResolution;
using Nimbus.IntegrationTests.Tests.LargeMessageTests.Handlers;
using Nimbus.IntegrationTests.Tests.LargeMessageTests.MessageContracts;
using Nimbus.LargeMessages.Azure.Client;
using Nimbus.Tests.Common.Configuration;
using Nimbus.Tests.Common.Stubs;
using Nimbus.Transports.WindowsServiceBus;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.LargeMessageTests
{
    [TestFixture]
    public class WhenSendingALargeRequestUsingAzureBlobStorage : SpecificationForAsync<Bus>
    {
        private BigFatResponse _response;
        private BigFatRequest _busRequest;

        protected override async Task<Bus> Given()
        {
            var typeProvider = new TestHarnessTypeProvider(new[] {GetType().Assembly}, new[] {GetType().Namespace});

            var bus = new BusBuilder().Configure()
                                      .WithTransport(new WindowsServiceBusTransportConfiguration()
                                                         .WithConnectionString(DefaultSettingsReader.Get<AzureServiceBusConnectionString>())
                                                         .WithLargeMessageStorage(new AzureBlobStorageLargeMessageStorageConfiguration()
                                                                                      .UsingStorageAccountConnectionString(
                                                                                          DefaultSettingsReader.Get<AzureBlobStorageConnectionString>())
                                                                                      .WithMaxSmallMessageSize(64*1024)
                                                                                      .WithMaxLargeMessageSize(10*1048576))
                )
                                      .WithNames("MyTestSuite", Environment.MachineName)
                                      .WithTypesFrom(typeProvider)
                                      .WithDependencyResolver(new DependencyResolver(typeProvider))
                                      .WithDefaultTimeout(TimeSpan.FromSeconds(TimeoutSeconds))
                                      .WithLogger(TestHarnessLoggerFactory.Create())
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
            _response = await Subject.Request(_busRequest, TimeSpan.FromSeconds(TimeoutSeconds));
        }

        [Test]
        public async Task TheResponseShouldReturnUnscathed()
        {
            _response.SomeBigAnswer.Length.ShouldBe(BigFatRequestHandler.MessageSize);
        }
    }
}