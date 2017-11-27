using System.Collections.Generic;
using Nimbus.Configuration.Debug.Settings;
using Nimbus.Configuration.PoorMansIocContainer;

namespace Nimbus.Configuration.Debug
{
    public class DebugConfiguration : INimbusConfiguration
    {
        internal RemoveAllExistingNamespaceElementsSetting RemoveAllExistingNamespaceElements { get; set; } = new RemoveAllExistingNamespaceElementsSetting();

        public void RegisterWith(PoorMansIoC container)
        {
            //FIXME add a startup hook here somehow to remove namespace components as per before.
        }

        public IEnumerable<string> Validate()
        {
            yield break;
        }
    }
}