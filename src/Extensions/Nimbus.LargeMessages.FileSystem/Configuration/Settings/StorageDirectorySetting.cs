using System.Collections.Generic;
using Nimbus.Configuration.Settings;

namespace Nimbus.LargeMessages.FileSystem.Configuration.Settings
{
    public class StorageDirectorySetting : Setting<string>
    {
        public override IEnumerable<string> Validate()
        {
            if (string.IsNullOrWhiteSpace(Value)) yield return "A storage directory must be provided.";
        }
    }
}