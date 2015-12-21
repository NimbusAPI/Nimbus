using System.Collections;
using System.Collections.Generic;
using ConfigInjector.QuickAndDirty;
using Nimbus.Configuration.Transport;
using Nimbus.Infrastructure.NimbusMessageServices.LargeMessages;
using Nimbus.Tests.Common.Configuration;
using Nimbus.Transports.InProcess;
using Nimbus.Transports.WindowsServiceBus;

namespace Nimbus.Tests.Common.TestScenarioGeneration
{
    public class TransportConfigurationSources : IEnumerable<PartialConfigurationScenario<TransportConfiguration>>
    {
        public IEnumerator<PartialConfigurationScenario<TransportConfiguration>> GetEnumerator()
        {
            yield return new PartialConfigurationScenario<TransportConfiguration>(
                nameof(InProcessTransportConfiguration),
                new InProcessTransportConfiguration());

            yield return new PartialConfigurationScenario<TransportConfiguration>(
                nameof(WindowsServiceBusTransportConfiguration),
                new WindowsServiceBusTransportConfiguration()
                    .WithConnectionString(DefaultSettingsReader.Get<AzureServiceBusConnectionString>())
                    .WithLargeMessageStorage(new UnsupportedLargeMessageBodyStorageConfiguration()),
                "Slow");

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