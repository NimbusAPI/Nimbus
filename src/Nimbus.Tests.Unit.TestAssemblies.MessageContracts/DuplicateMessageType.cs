using Nimbus.MessageContracts;

namespace Nimbus.Tests.Unit.TestAssemblies.MessageContracts
{
    public class DuplicateMessageType : IBusEvent
    {
    }

    public class duplicateMessageType : IBusEvent
    {
    }
}