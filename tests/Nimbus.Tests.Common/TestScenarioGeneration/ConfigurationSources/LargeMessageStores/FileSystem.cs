using System;
using System.IO;
using Nimbus.Configuration.LargeMessages;
using Nimbus.LargeMessages.FileSystem.Configuration;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.LargeMessageStores
{
    internal class FileSystem : ConfigurationScenario<LargeMessageStorageConfiguration>
    {
        public override ScenarioInstance<LargeMessageStorageConfiguration> CreateInstance()
        {
            var largeMessageBodyTempPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                                        "Nimbus Integration Test Suite",
                                                        Guid.NewGuid().ToString());

            var configuration = new FileSystemStorageConfiguration()
                .WithStorageDirectory(largeMessageBodyTempPath);

            var instance = new ScenarioInstance<LargeMessageStorageConfiguration>(configuration);
            instance.Disposing += (s, e) => Directory.Delete(largeMessageBodyTempPath, true);

            return instance;
        }
    }
}