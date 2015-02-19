using System;
using System.Collections.Generic;

namespace Nimbus.Configuration.Settings
{
    public class TopicAutoDeleteOnIdleSetting : Setting<TimeSpan>
    {
        public override TimeSpan Default
        {
            get { return TimeSpan.FromDays(367); }
        }

        public override IEnumerable<string> Validate()
        {
            if (Value < TimeSpan.FromMinutes(5)) yield return "The minimum duration is 5 minutes.";
        }
    }
}