using Nimbus.Configuration.Settings;

namespace Nimbus.Configuration.LargeMessages.Settings
{
    public class MaxSmallMessageSizeSetting : Setting<int>
    {
        public override int Default
        {
            get { return 64*1024; }
        }
    }
}