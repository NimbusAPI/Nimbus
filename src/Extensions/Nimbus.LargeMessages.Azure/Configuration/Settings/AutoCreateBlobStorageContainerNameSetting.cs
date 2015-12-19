using System.Collections.Generic;
using Nimbus.Configuration.Settings;

namespace Nimbus.LargeMessages.Azure.Configuration.Settings
{
    public class AutoCreateBlobStorageContainerNameSetting : Setting<string>
    {
        public override string Default
        {
            get { return "messagebodies"; }
        }

        public override IEnumerable<string> Validate()
        {
            if (string.IsNullOrWhiteSpace(Value)) yield return "No value was provided.";
        }
    }
}