using System;
using Nimbus.Configuration.PoorMansIocContainer;

namespace Nimbus.Configuration.Transport
{
    internal class NotSupportedTransportConfiguration : TransportConfiguration
    {
        protected override void RegisterComponents(PoorMansIoC container)
        {
            throw new NotSupportedException("You must specify a transport layer.");
        }
    }
}