using Nimbus.MessageContracts.ControlMessages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Configuration.Settings
{
    public class HeartbeatTransformSetting : Setting<Func<HeartbeatEvent, HeartbeatEvent>>
    {
        public override Func<HeartbeatEvent, HeartbeatEvent> Default
        {
            get { return beat => beat; }
        }
    }
}
