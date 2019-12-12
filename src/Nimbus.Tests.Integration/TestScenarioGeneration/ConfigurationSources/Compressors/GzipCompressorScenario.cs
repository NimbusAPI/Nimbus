using Nimbus.Infrastructure.Compression;
using Nimbus.InfrastructureContracts;
using Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition;

namespace Nimbus.Tests.Integration.TestScenarioGeneration.ConfigurationSources.Compressors
{
    public class GzipCompressorScenario : ConfigurationScenario<ICompressor>
    {
        public override ScenarioInstance<ICompressor> CreateInstance()
        {
            var compressor = new GzipCompressor();
            var instance = new ScenarioInstance<ICompressor>(compressor);
            return instance;
        }
    }
}