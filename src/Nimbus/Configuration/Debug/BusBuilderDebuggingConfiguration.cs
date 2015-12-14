using Nimbus.Configuration.Debug.Settings;
using Nimbus.Configuration.PoorMansIocContainer;

namespace Nimbus.Configuration.Debug
{
    public class BusBuilderDebuggingConfiguration : INimbusConfiguration
    {
        internal RemoveAllExistingNamespaceElementsSetting RemoveAllExistingNamespaceElements { get; set; }

        public void Register(PoorMansIoC container)
        {
        }
    }
}