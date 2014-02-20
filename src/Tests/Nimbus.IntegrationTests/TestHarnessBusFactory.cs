using System;
using System.Threading;
using Nimbus.Configuration;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;
using Nimbus.Logger;

namespace Nimbus.IntegrationTests
{
    public static class TestHarnessBusFactory
    {
        public static Bus CreateAndStart(ITypeProvider typeProvider, DefaultMessageHandlerFactory messageHandlerFactory)
        {
            var logger = new ConsoleLogger();

            var bus = new BusBuilder().Configure()
                                      .WithNames("MyTestSuite", Environment.MachineName)
                                      .WithConnectionString(CommonResources.ConnectionString)
                                      .WithTypesFrom(typeProvider)
                                      .WithCommandHandlerFactory(messageHandlerFactory)
                                      .WithRequestBroker(messageHandlerFactory)
                                      .WithMulticastEventBroker(messageHandlerFactory)
                                      .WithCompetingEventBroker(messageHandlerFactory)
                                      .WithMulticastRequestBroker(messageHandlerFactory)
                                      .WithDefaultTimeout(TimeSpan.FromSeconds(10))
                                      .WithLogger(logger)
                                      .WithDebugOptions(
                                          dc =>
                                              dc.RemoveAllExistingNamespaceElementsOnStartup(
                                                  "I understand this will delete EVERYTHING in my namespace. I promise to only use this for test suites."))
                                      .Build();
            bus.Start();
            return bus;
        }
    }
}