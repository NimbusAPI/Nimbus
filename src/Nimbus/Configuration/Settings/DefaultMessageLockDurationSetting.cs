using System;

namespace Nimbus.Configuration.Settings
{
    [Obsolete("Damian, I'll leave you to do the honours :)")]
    public class DefaultMessageLockDurationSetting : Setting<TimeSpan>
    {
        public override TimeSpan Default
        {
            get { return TimeSpan.FromSeconds(30); }
        }
    }
}