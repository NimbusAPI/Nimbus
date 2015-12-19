using Nimbus.Configuration.PoorMansIocContainer;

namespace Nimbus.Configuration
{
    public interface INimbusConfiguration: IValidatableConfigurationSetting
    {
        void RegisterWith(PoorMansIoC container);
    }
}