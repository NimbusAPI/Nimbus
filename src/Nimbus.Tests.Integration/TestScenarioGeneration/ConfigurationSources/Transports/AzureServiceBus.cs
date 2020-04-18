using System.Collections.Generic;
using Nimbus.Configuration.LargeMessages;
using Nimbus.Configuration.Transport;
using Nimbus.Tests.Integration.Configuration;
using Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition;
using Nimbus.Transports.AzureServiceBus;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.Transports
{
    internal class AzureServiceBus : ConfigurationScenario<TransportConfiguration>
    {
        
        protected override IEnumerable<string> AdditionalCategories
        {
            get { yield return "Slow"; }
        }

        public override ScenarioInstance<TransportConfiguration> CreateInstance()
        {
            //var largeMessageStorageInstance = _largeMessageScenario.CreateInstance();

            var azureServiceBusConnectionString =  AppSettingsLoader.Settings.Transports.AzureServiceBus.ConnectionString;
            var configuration = new AzureServiceBusTransportConfiguration()
                .WithConnectionString(azureServiceBusConnectionString);
                //.WithLargeMessageStorage(largeMessageStorageInstance.Configuration);

            var instance = new ScenarioInstance<TransportConfiguration>(configuration);

            return instance;
        }
    }
}