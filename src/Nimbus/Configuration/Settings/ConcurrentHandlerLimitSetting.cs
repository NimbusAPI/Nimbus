using System;

namespace Nimbus.Configuration.Settings
{
    public class ConcurrentHandlerLimitSetting: Setting<int>
    {
        public override int Default
        {
            get { return Environment.ProcessorCount * 2; }
        }
    }
}