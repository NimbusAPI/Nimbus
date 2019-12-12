using Nimbus.MessageContracts;

namespace Nimbus.StressTests.ThreadStarvationTests.BadlyBehavedHandlersThatDoNotKnowAboutAsync.MessageContracts
{
    public class CommandThatWillBlockTheThread : IBusCommand
    {
    }
}