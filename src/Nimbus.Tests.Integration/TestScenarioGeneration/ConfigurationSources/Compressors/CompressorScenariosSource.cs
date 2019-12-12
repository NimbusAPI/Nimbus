using System.Collections;
using System.Collections.Generic;
using Nimbus.InfrastructureContracts;
using Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition;

namespace Nimbus.Tests.Integration.TestScenarioGeneration.ConfigurationSources.Compressors
{
    public class CompressorScenariosSource : IEnumerable<ConfigurationScenario<ICompressor>>
    {
        public IEnumerator<ConfigurationScenario<ICompressor>> GetEnumerator()
        {
            yield return new NullCompressorScenario();
            yield return new GzipCompressorScenario();
            yield return new DeflateCompressorScenario();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}