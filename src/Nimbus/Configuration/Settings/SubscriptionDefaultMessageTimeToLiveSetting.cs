using System;

namespace Nimbus.Configuration.Settings
{
    public class SubscriptionDefaultMessageTimeToLiveSetting : Setting<TimeSpan>
    {
        public override TimeSpan Default
        {
            get { return TimeSpan.MaxValue; }
        }
    }
}