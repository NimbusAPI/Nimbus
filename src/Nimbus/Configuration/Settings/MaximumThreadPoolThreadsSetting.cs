using System;

namespace Nimbus.Configuration.Settings
{
    public class MaximumThreadPoolThreadsSetting : Setting<int>
    {
        public override int Default
        {
            get
            {
                var absoluteMinimumThreadCount = Math.Max(Environment.ProcessorCount, 4);
                var maximumThreadCount = absoluteMinimumThreadCount*absoluteMinimumThreadCount;
                return maximumThreadCount;
            }
        }
    }
}