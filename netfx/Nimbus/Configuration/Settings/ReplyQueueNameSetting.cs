using Nimbus.Infrastructure;
using Nimbus.Routing;

namespace Nimbus.Configuration.Settings
{
    public class ReplyQueueNameSetting : Setting<string>
    {
        public ReplyQueueNameSetting(ApplicationNameSetting applicationName, InstanceNameSetting instanceName, IPathFactory pathFactory)
        {
            Value = pathFactory.InputQueuePathFor(applicationName, instanceName);
        }
    }
}