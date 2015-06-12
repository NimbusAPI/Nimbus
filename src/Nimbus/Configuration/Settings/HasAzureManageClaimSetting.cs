namespace Nimbus.Configuration.Settings
{
    public class HasAzureManageClaimSetting : Setting<bool>
    {
        public override bool Default
        {
            get { return true; }
        }
    }
}