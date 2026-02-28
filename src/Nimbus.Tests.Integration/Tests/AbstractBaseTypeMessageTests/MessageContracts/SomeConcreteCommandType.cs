namespace Nimbus.Tests.Integration.Tests.AbstractBaseTypeMessageTests.MessageContracts
{
    /// <summary>
    ///     We use this message type to confirm that our AssemblyScanningTypeProvider can handle message
    ///     types that don't implement the IBusCommand/Event/Request/Response interfaces directly.
    /// </summary>
    public class SomeConcreteCommandType : SomeAbstractCommandType
    {
    }
}