using System;

namespace Nimbus.Configuration.Settings
{
    public class MinimumThreadPoolThreadsSetting : Setting<int>
    {
        public override int Default
        {
            get
            {
                var absoluteMinimumThreadCount = Math.Max(Environment.ProcessorCount, 4);
                return absoluteMinimumThreadCount;
            }
        }
    }
}