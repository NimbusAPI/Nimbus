namespace Nimbus.Configuration.Debug
{
    public class BusBuilderDebuggingConfiguration: INimbusConfiguration
    {
        internal bool RemoveAllExistingNamespaceElements { get; set; }
        internal bool UseInProcessBus { get; set; }
    }
}