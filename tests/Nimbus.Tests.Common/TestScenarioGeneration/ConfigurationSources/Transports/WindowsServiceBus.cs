using System.Collections.Generic;
using ConfigInjector.QuickAndDirty;
using Nimbus.Configuration.LargeMessages;
using Nimbus.Configuration.Transport;
using Nimbus.Tests.Common.Configuration;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;
using Nimbus.Transports.WindowsServiceBus;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.Transports
{
    internal class WindowsServiceBus : CompositeScenario, IConfigurationScenario<TransportConfiguration>
    {
        private readonly IConfigurationScenario<LargeMessageStorageConfiguration> _largeMessageScenario;

        public WindowsServiceBus(IConfigurationScenario<LargeMessageStorageConfiguration> largeMessageScenario)
            : base(largeMessageScenario)
        {
            _largeMessageScenario = largeMessageScenario;
        }

        public WindowsServiceBus()
        {
            _largeMessageScenario = null;
        }

        protected override IEnumerable<string> AdditionalCategories
        {
            get { yield return "Slow"; }
        }

        public ScenarioInstance<TransportConfiguration> CreateInstance()
        {
            var azureServiceBusConnectionString = DefaultSettingsReader.Get<AzureServiceBusConnectionString>();
            var configuration = new WindowsServiceBusTransportConfiguration()
                .WithConnectionString(azureServiceBusConnectionString);

            if(_largeMessageScenario != null)
            {
                var largeMessageStorageInstance = _largeMessageScenario.CreateInstance();
                configuration.WithLargeMessageStorage(largeMessageStorageInstance.Configuration);
            }

            var instance = new ScenarioInstance<TransportConfiguration>(configuration);

            return instance;
        }
    }
}