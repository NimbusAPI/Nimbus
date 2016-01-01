using System.Diagnostics;
using System.Threading.Tasks;
using Nimbus.StressTests.ThroughputTests.EventHandlers;
using Nimbus.StressTests.ThroughputTests.MessageContracts;
using NUnit.Framework;

namespace Nimbus.StressTests.ThroughputTests
{
    [TestFixture]
    public class WhenSendingManyIndividualRequestsOfTheSameType : ThroughputSpecificationForBus
    {
        public override async Task SendMessages(IBus bus)
        {
            var sw = Stopwatch.StartNew();
            while (sw.Elapsed < SendMessagesFor)
            {
                var response = await bus.Request(new FooRequest());
                IncrementExpectedMessageCount();
                StressTestMessageHandler.RecordResponseMessageReceipt(response);
            }
        }
    }
}