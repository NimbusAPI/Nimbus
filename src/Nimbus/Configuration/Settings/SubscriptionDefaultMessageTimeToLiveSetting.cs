using System;

namespace Nimbus.Configuration.Settings
{
    public class DefaultMessageTimeToLiveSetting : Setting<TimeSpan>
    {
        public override TimeSpan Default => TimeSpan.MaxValue;
    }
}