using Nimbus.Infrastructure.Compression;
using Nimbus.InfrastructureContracts;
using Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition;

namespace Nimbus.Tests.Integration.TestScenarioGeneration.ConfigurationSources.Compressors
{
    public class DeflateCompressorScenario : ConfigurationScenario<ICompressor>
    {
        public override ScenarioInstance<ICompressor> CreateInstance()
        {
            var compressor = new DeflateCompressor();
            var instance = new ScenarioInstance<ICompressor>(compressor);
            return instance;
        }
    }
}