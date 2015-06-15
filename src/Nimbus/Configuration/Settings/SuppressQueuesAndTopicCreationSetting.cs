namespace Nimbus.Configuration.Settings
{
    public class SuppressQueuesAndTopicCreationSetting : Setting<bool>
    {
        public override bool Default
        {
            get { return false; }
        }
    }
}