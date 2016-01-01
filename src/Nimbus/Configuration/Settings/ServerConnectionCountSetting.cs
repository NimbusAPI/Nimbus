using System;
using System.Collections.Generic;

namespace Nimbus.Configuration.Settings
{
    public class ServerConnectionCountSetting : Setting<int>
    {
        public override int Default => Math.Min(4, Environment.ProcessorCount);

        public override IEnumerable<string> Validate()
        {
            if (Value <= 0) yield return "You must permit at least one server connection if you want to talk to anybody.";
        }
    }
}