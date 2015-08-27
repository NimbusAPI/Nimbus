namespace Nimbus.Configuration.Settings
{
    public class AutoRecreateMessagingEntitySetting : Setting<bool>
    {
        public override bool Default
        {
            get { return true; }
        }
    }
}