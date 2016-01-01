using System.Collections;
using System.Collections.Generic;
using Nimbus.Configuration.Transport;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.Transports
{
    public class TransportConfigurationSources : IEnumerable<IConfigurationScenario<TransportConfiguration>>
    {
        public IEnumerator<IConfigurationScenario<TransportConfiguration>> GetEnumerator()
        {
            yield return new InProcess();
            yield return new WindowsServiceBus();

            //FIXME: how many levels of nesting would we like? :)
            //foreach (var largeMessageStorage in new LargeMessageStorageConfigurationSources())
            //{
            //    yield return new PartialConfigurationScenario<TransportConfiguration>(
            //        PartialConfigurationScenario.Combine(nameof(WindowsServiceBusTransportConfiguration), largeMessageStorage.Name),
            //        new WindowsServiceBusTransportConfiguration()
            //            .WithConnectionString(DefaultSettingsReader.Get<AzureServiceBusConnectionString>())
            //            .WithLargeMessageStorage(largeMessageStorage.Configuration));
            //}
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}