using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Infrastructure;
using Nimbus.IntegrationTests.Tests.LargeMessageTests.Handlers;
using Nimbus.IntegrationTests.Tests.LargeMessageTests.MessageContracts;
using Nimbus.LargeMessages.Azure.Configuration;
using Nimbus.Logger;
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
            var messageHandlerFactory = new DefaultMessageHandlerFactory(typeProvider);
            var logger = new ConsoleLogger();
            var bus = new BusBuilder().Configure()
                                      .WithNames("MyTestSuite", Environment.MachineName)
                                      .WithConnectionString(CommonResources.ServiceBusConnectionString)
                                      .WithTypesFrom(typeProvider)
                                      .WithDefaultHandlerFactory(messageHandlerFactory)
                                      .WithDefaultTimeout(TimeSpan.FromSeconds(10))
                                      .WithLogger(logger)
                                      .WithAzureMessageBodyStorage(c => c.WithBlobStorageConnectionString(CommonResources.BlobStorageConnectionString)
                                                                         .WithMaxSmallMessageSize(64*1024)
                                                                         .WithMaxLargeMessageSize(10*1048576))
                                      .WithDebugOptions(dc => dc.RemoveAllExistingNamespaceElementsOnStartup(
                                          "I understand this will delete EVERYTHING in my namespace. I promise to only use this for test suites."))
                                      .Build();
            bus.Start();
            return bus;
        }

        protected override async Task When()
        {
            var bigQuestion = new string(Enumerable.Range(0, BigFatRequestHandler.MessageSize).Select(i => '.').ToArray());

            _busRequest = new BigFatRequest
                          {
                              SomeBigQuestion = bigQuestion,
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