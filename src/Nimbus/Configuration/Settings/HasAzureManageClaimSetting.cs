namespace Nimbus.Configuration.Settings
{
    internal class HasAzureManageClaimSetting : Setting<bool>
    {
        public override bool Default
        {
            get { return true; }
        }
    }
}