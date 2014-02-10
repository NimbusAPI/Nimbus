using Nimbus.Infrastructure;

namespace Nimbus.Configuration.Settings
{
    public class ReplyQueueNameSetting : Setting<string>
    {
        public ReplyQueueNameSetting(ApplicationNameSetting applicationName, InstanceNameSetting instanceName)
        {
            Value = PathFactory.InputQueuePathFor(applicationName, instanceName);
        }
    }
}