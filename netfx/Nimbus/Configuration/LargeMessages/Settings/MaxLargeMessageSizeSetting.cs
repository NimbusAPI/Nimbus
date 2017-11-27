using Nimbus.Configuration.Settings;

namespace Nimbus.Configuration.LargeMessages.Settings
{
    public class MaxLargeMessageSizeSetting : Setting<int>
    {
        public override int Default
        {
            get { return 10*1048576; }
        }
    }
}