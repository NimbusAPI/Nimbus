namespace Nimbus.Configuration.Settings
{
    public class ServerConnectionCountSetting : Setting<int>
    {
        public override int Default
        {
            get { return 20; }
        }
    }
}