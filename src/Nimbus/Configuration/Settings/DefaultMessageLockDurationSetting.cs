using System;

namespace Nimbus.Configuration.Settings
{
    public class DefaultMessageLockDurationSetting : Setting<TimeSpan>
    {
        public override TimeSpan Default
        {
            get { return TimeSpan.FromSeconds(30); }
        }
    }
}