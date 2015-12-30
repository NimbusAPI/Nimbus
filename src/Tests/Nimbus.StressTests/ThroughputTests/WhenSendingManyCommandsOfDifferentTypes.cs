using System.Diagnostics;
using System.Threading.Tasks;
using Nimbus.StressTests.ThroughputTests.MessageContracts;
using NUnit.Framework;

namespace Nimbus.StressTests.ThroughputTests
{
    [TestFixture]
    public class WhenSendingManyCommandsOfDifferentTypes : ThroughputSpecificationForBus
    {
        public override async Task SendMessages(IBus bus)
        {
            var sw = Stopwatch.StartNew();
            while (sw.Elapsed < SendMessagesFor)
            {
                await bus.Send(new FooCommand());
                await bus.Send(new BarCommand());
                await bus.Send(new BazCommand());
                await bus.Send(new QuxCommand());
                IncrementExpectedMessageCount(4);
            }
        }
    }
}