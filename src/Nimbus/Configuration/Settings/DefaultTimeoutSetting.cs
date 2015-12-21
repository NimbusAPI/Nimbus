using System;

namespace Nimbus.Configuration.Settings
{
    public class DefaultTimeoutSetting : Setting<TimeSpan>
    {
        public override TimeSpan Default => TimeSpan.FromSeconds(60);
    }
}