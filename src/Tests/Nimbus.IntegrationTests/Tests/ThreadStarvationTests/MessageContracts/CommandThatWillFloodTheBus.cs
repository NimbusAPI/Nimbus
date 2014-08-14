using Nimbus.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.ThreadStarvationTests.MessageContracts
{
    public class CommandThatWillFloodTheBus : IBusCommand
    {
    }
}