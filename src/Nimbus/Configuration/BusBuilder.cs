using System;
using System.Threading.Tasks;
using Nimbus.Configuration.Debug.Settings;
using Nimbus.Configuration.PoorMansIocContainer;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.Commands;
using Nimbus.Infrastructure.Events;
using Nimbus.Infrastructure.PropertyInjection;
using Nimbus.Infrastructure.RequestResponse;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Configuration
{
    public class BusBuilder
    {
        public BusBuilderConfiguration Configure()
        {
            return new BusBuilderConfiguration();
        }

        internal static Bus Build(BusBuilderConfiguration configuration)
        {
            var logger = configuration.Logger;
            logger.Debug("Constructing bus...");

            configuration.AssertConfigurationIsValid();

            var container = new PoorMansIoC();
            container.RegisterPropertiesFromConfigurationObject(configuration);

            logger.Debug("Creating message pumps...");
            var messagePumpsManager = new MessagePumpsManager(
                container.Resolve<ResponseMessagePumpFactory>().Create(),
                container.Resolve<RequestMessagePumpsFactory>().CreateAll(),
                container.Resolve<CommandMessagePumpsFactory>().CreateAll(),
                container.Resolve<MulticastRequestMessagePumpsFactory>().CreateAll(),
                container.Resolve<MulticastEventMessagePumpsFactory>().CreateAll(),
                container.Resolve<CompetingEventMessagePumpsFactory>().CreateAll());

            logger.Debug("Message pumps are all created.");

            var bus = container.ResolveWithOverrides<Bus>(messagePumpsManager);
            container.Resolve<PropertyInjector>().Bus = bus;

            bus.Starting += (s, e) =>
                            {
                                Task.Run(async () =>
                                               {
                                                   try
                                                   {
                                                       await container.Resolve<INimbusTransport>().TestConnection();
                                                       await CleanNamespace(container, logger);
                                                   }
                                                   catch (Exception ex)
                                                   {
                                                       logger.Error(ex, "Failed to establish connection to underlying transport: {Message}", ex.Message);
                                                       throw;
                                                   }
                                               }).Wait();
                            };

            bus.Disposing += (s, e) =>
                             {
                                 Task.Run(async () =>
                                                {
                                                    await CleanNamespace(container, logger);
                                                    container.Dispose();
                                                }).Wait();
                             };

            logger.Info("Bus built. Job done!");

            return bus;
        }

        private static async Task CleanNamespace(PoorMansIoC container, ILogger logger)
        {
            var removeAllExistingElements = container.Resolve<RemoveAllExistingNamespaceElementsSetting>();
            if (!removeAllExistingElements) return;

            logger.Debug("Removing all existing namespace elements. IMPORTANT: This should only be done in your regression test suites.");
            var cleanser = container.Resolve<INamespaceCleanser>();
            await cleanser.RemoveAllExistingNamespaceElements();
            logger.Debug("Namespace elements removed");
        }
    }
}