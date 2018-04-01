using System.Collections;
using System.Collections.Generic;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.Compressors
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