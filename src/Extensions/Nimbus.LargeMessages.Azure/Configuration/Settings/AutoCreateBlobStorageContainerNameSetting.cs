using Nimbus.Configuration.Settings;

namespace Nimbus.LargeMessages.Azure.Configuration.Settings
{
    public class AutoCreateBlobStorageContainerNameSetting : Setting<string>
    {
        public override string Default
        {
            get { return "messagebodies"; }
        }
    }
}