namespace Nimbus.Configuration.Settings
{
    public class ReplyQueueNameSetting : Setting<string>
    {
        public ReplyQueueNameSetting(ApplicationNameSetting applicationName, InstanceNameSetting instanceName)
        {
            Value = string.Format("InputQueue.{0}.{1}", applicationName, instanceName); 
        }
    }
}