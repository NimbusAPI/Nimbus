using ConfigInjector.QuickAndDirty;
using Nimbus.Configuration.Transport;
using Nimbus.Infrastructure.LargeMessages;
using Nimbus.Tests.Common.Configuration;
using Nimbus.Transports.WindowsServiceBus;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.Transports
{
    internal class WindowsServiceBus : IConfigurationScenario<TransportConfiguration>
    {
        public string Name { get; } = "WindowsServiceBus";
        public string[] Categories { get; } = {"WindowsServiceBus", "Slow"};

        public ScenarioInstance<TransportConfiguration> CreateInstance()
        {
            var azureServiceBusConnectionString = DefaultSettingsReader.Get<AzureServiceBusConnectionString>();
            var configuration = new WindowsServiceBusTransportConfiguration()
                .WithConnectionString(azureServiceBusConnectionString)
                .WithLargeMessageStorage(new UnsupportedLargeMessageBodyStorageConfiguration());

            var instance = new ScenarioInstance<TransportConfiguration>(configuration);

            return instance;
        }
    }
}