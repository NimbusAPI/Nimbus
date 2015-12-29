using Nimbus.Configuration.Debug.Settings;
using Nimbus.Configuration.PoorMansIocContainer;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.Commands;
using Nimbus.Infrastructure.Events;
using Nimbus.Infrastructure.PropertyInjection;
using Nimbus.Infrastructure.RequestResponse;

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

            bus.Starting += delegate
                            {
                                container.Resolve<INimbusTransport>().TestConnection().Wait();

                                var removeAllExistingElements = container.Resolve<RemoveAllExistingNamespaceElementsSetting>();
                                if (removeAllExistingElements)
                                {
                                    var cleanser = container.Resolve<INamespaceCleanser>();
                                    cleanser.RemoveAllExistingNamespaceElements().Wait();
                                }
                            };
            bus.Disposing += delegate { container.Dispose(); };

            logger.Info("Bus built. Job done!");

            return bus;
        }
    }
}