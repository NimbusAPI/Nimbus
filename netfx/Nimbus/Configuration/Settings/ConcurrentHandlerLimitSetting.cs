using System;

namespace Nimbus.Configuration.Settings
{
    public class ConcurrentHandlerLimitSetting : Setting<int>
    {
        public override int Default => Environment.ProcessorCount*2;
    }
}