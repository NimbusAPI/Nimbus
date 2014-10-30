namespace Nimbus.Configuration.Settings
{
    public class EnableDeadLetteringOnMessageExpirationSetting : Setting<bool>
    {
        public override bool Default
        {
            get { return true; }
        }
    }
}