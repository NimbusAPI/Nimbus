using Nimbus.MessageContracts;

namespace Nimbus.Tests.TestAssemblies.MessageContracts
{
    /// <summary>
    ///     We use this message type to test our AssemblyScanningTypeProvider by having a handler for this
    ///     type but not actually including this type's assembly in the list of assemblies that we ask the
    ///     type provider to scan.  -andrewh 9/12/2013
    /// </summary>
    public class CommandWhoseAssemblyShouldNotBeIncluded : IBusCommand
    {
    }
}