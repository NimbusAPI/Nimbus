using System;
using System.Collections.Generic;

namespace Nimbus.Configuration.Settings
{
    public class FetchExistingTopicsTimeoutSetting : Setting<TimeSpan>
    {
        public override TimeSpan Default
        {
            get { return TimeSpan.FromSeconds(10); }
        }

        public override IEnumerable<string> Validate()
        {
            if (Value < TimeSpan.FromSeconds(1)) yield return "The minimum duration is 1 second.";
        }
    }
}