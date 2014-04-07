using System;
using Nimbus.Configuration;
using Nimbus.Infrastructure;
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
                                      .WithConnectionString(CommonResources.ServiceBusConnectionString)
                                      .WithTypesFrom(typeProvider)
                                      .WithDefaultHandlerFactory(messageHandlerFactory)
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