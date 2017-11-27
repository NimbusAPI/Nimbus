using System;
using System.Collections.Generic;
using Nimbus.Configuration.PoorMansIocContainer;

namespace Nimbus.Configuration.Transport
{
    [Obsolete]
    internal class NotSupportedTransportConfiguration : TransportConfiguration
    {
        protected override void RegisterComponents(PoorMansIoC container)
        {
        }

        public override IEnumerable<string> Validate()
        {
            yield return "You must specify a transport later.";
        }
    }
}