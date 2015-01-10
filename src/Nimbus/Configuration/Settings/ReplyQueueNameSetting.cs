using Nimbus.Routing;

namespace Nimbus.Configuration.Settings
{
    public class ReplyQueueNameSetting : Setting<string>
    {
        public ReplyQueueNameSetting(
            ApplicationNameSetting applicationName, 
            InstanceNameSetting instanceName, 
            IPathGenerator pathGenerator)
        {
            Value = pathGenerator.InputQueuePathFor(applicationName, instanceName);
        }
    }
}