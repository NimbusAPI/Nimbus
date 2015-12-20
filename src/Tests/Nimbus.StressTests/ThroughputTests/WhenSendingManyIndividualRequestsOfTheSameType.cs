using System.Threading.Tasks;
using Nimbus.StressTests.ThroughputTests.EventHandlers;
using Nimbus.StressTests.ThroughputTests.MessageContracts;
using NUnit.Framework;

namespace Nimbus.StressTests.ThroughputTests
{
    [TestFixture]
    public class WhenSendingManyIndividualRequestsOfTheSameType : ThroughputSpecificationForBus
    {
        protected override int NumMessagesToSend => 100;

        public override async Task SendMessages(IBus bus)
        {
            for (var i = 0; i < NumMessagesToSend; i++)
            {
                var response = await bus.Request(new FooRequest());
                StressTestMessageHandler.RecordResponseMessageReceipt(response);
            }
        }
    }
}