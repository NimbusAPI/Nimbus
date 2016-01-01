using Nimbus.MessageContracts;

namespace Nimbus.StressTests.ThroughputTests.MessageContracts
{
    public class QuxCommand : StressTestMessage, IBusCommand
    {
    }
}