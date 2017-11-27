using System.Collections.Generic;

namespace Nimbus.Configuration.Settings
{
    public class MaxDeliveryAttemptSetting : Setting<int>
    {
        public override int Default
        {
            get { return 5; }
        }

        public override IEnumerable<string> Validate()
        {
            if (Value < 1) yield return "You must attempt to deliver a message at least once.";
        }
    }
}