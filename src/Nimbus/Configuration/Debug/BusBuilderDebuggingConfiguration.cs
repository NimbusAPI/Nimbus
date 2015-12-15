using Nimbus.Configuration.Debug.Settings;
using Nimbus.Configuration.PoorMansIocContainer;

namespace Nimbus.Configuration.Debug
{
    public class BusBuilderDebuggingConfiguration : INimbusConfiguration
    {
        internal RemoveAllExistingNamespaceElementsSetting RemoveAllExistingNamespaceElements { get; set; }

        public void RegisterWith(PoorMansIoC container)
        {
            //FIXME add a startup hook here somehow to remove namespace components as per before.
        }
    }
}