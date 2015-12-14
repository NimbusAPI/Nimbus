using System;
using Nimbus.Configuration.PoorMansIocContainer;

namespace Nimbus.Configuration.Transport
{
    public abstract class TransportConfiguration: INimbusConfiguration
    {
        public void Register(PoorMansIoC container)
        {
            RegisterComponents(container);
            //FIXME assert that a transport was actually registered
        }

        protected abstract void RegisterComponents(PoorMansIoC container);
    }
}