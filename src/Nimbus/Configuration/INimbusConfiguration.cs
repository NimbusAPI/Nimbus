using Nimbus.Configuration.PoorMansIocContainer;

namespace Nimbus.Configuration
{
    public interface INimbusConfiguration
    {
        void RegisterWith(PoorMansIoC container);
    }
}