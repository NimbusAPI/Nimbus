using Nimbus.MessageContracts;

namespace Nimbus.StressTests.ThreadStarvationTests.MessageContracts
{
    public class CommandThatWillBlockTheThread : IBusCommand
    {
    }
}