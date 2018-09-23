using System.Collections.Generic;
using ConfigInjector.QuickAndDirty;
using Nimbus.Configuration.LargeMessages;
using Nimbus.Configuration.Transport;
using Nimbus.Tests.Common.Configuration;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;
using Nimbus.Transports.AzureServiceBus;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.Transports
{
    internal class AzureServiceBus : CompositeScenario, IConfigurationScenario<TransportConfiguration>
    {
        private readonly IConfigurationScenario<LargeMessageStorageConfiguration> _largeMessageScenario;

        public AzureServiceBus(IConfigurationScenario<LargeMessageStorageConfiguration> largeMessageScenario)
            : base(largeMessageScenario)
        {
            _largeMessageScenario = largeMessageScenario;
        }

        protected override IEnumerable<string> AdditionalCategories
        {
            get { yield return "Slow"; }
        }

        public ScenarioInstance<TransportConfiguration> CreateInstance()
        {
            var largeMessageStorageInstance = _largeMessageScenario.CreateInstance();

            var azureServiceBusConnectionString = DefaultSettingsReader.Get<AzureServiceBusConnectionString>();
            var configuration = new AzureServiceBusTransportConfiguration()
                .WithConnectionString(azureServiceBusConnectionString)
                .WithLargeMessageStorage(largeMessageStorageInstance.Configuration);

            var instance = new ScenarioInstance<TransportConfiguration>(configuration);

            return instance;
        }
    }
}