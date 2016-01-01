using System;

namespace Nimbus.Configuration.Settings
{
    public class GlobalConcurrentHandlerLimitSetting : Setting<int>
    {
        public override int Default => Environment.ProcessorCount*4;
    }
}