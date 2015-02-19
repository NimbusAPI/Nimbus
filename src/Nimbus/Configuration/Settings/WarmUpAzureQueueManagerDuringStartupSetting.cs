namespace Nimbus.Configuration.Settings
{
    public class WarmUpAzureQueueManagerDuringStartupSetting : Setting<bool>
    {
        public override bool Default
        {
            get
            {
                return true;
            }
        }
    }
}