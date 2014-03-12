namespace Nimbus.Configuration.Settings
{
    public class MaxDeliveryAttemptSetting : Setting<int>
    {
        public override int Default
        {
            get { return 5; }
        }
    }
}