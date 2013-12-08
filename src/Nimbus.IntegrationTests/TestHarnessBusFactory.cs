using System;
using Nimbus.Configuration;
using Nimbus.InfrastructureContracts;
using Nimbus.Logger;

namespace Nimbus.IntegrationTests
{
    public static class TestHarnessBusFactory
    {
        public static Bus Create(ITypeProvider typeProvider, TestHarnessMessageBroker messageBroker)
        {
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
            bus.Start();
            return bus;
        }
    }
}