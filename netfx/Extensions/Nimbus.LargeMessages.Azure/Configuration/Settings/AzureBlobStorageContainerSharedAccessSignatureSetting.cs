using System.Collections.Generic;
using Nimbus.Configuration.Settings;

namespace Nimbus.LargeMessages.Azure.Configuration.Settings
{
    public class AzureBlobStorageContainerSharedAccessSignatureSetting : Setting<string>
    {
        public override IEnumerable<string> Validate()
        {
            if (string.IsNullOrWhiteSpace(Value)) yield return "No value was provided.";
        }
    }
}