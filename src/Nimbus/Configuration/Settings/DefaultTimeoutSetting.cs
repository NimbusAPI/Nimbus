using System;

namespace Nimbus.Configuration.Settings
{
    public class DefaultTimeoutSetting : Setting<TimeSpan>
    {
        public override TimeSpan Default
        {
            get { return TimeSpan.FromSeconds(60); }
        }
    }
}