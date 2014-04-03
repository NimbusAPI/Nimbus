namespace Nimbus.Configuration.Settings
{
    public class GzipMessageCompressionSetting : Setting<bool>
    {
        public override bool Default
        {
            get { return false; }
        }
    }
}