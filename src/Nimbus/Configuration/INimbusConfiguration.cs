using Nimbus.Configuration.PoorMansIocContainer;

namespace Nimbus.Configuration
{
    public interface INimbusConfiguration
    {
        void Register(PoorMansIoC container);
    }
}