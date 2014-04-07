namespace Nimbus.Configuration.Settings
{
    public class MaxLargeMessageSizeSetting : Setting<int>
    {
        public override int Default
        {
            get { return 10*1048576; }
        }
    }
}