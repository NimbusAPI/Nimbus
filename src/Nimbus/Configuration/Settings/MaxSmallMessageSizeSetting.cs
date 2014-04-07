namespace Nimbus.Configuration.Settings
{
    public class MaxSmallMessageSizeSetting : Setting<int>
    {
        public override int Default
        {
            get { return 64*1024; }
        }
    }
}