using System;
using System.Collections.Generic;
using Nimbus.Configuration.Settings;

namespace Nimbus.LargeMessages.Azure.Configuration.Settings
{
    public class AzureBlobStorageContainerUriSetting : Setting<Uri>
    {
        public override IEnumerable<string> Validate()
        {
            if (Value == default(Uri)) yield return "No value was provided.";
        }
    }
}