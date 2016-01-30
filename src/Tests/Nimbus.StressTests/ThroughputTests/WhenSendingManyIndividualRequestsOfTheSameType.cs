using System;
using System.Threading.Tasks;
using Nimbus.StressTests.ThroughputTests.EventHandlers;
using Nimbus.StressTests.ThroughputTests.MessageContracts;
using NUnit.Framework;

namespace Nimbus.StressTests.ThroughputTests
{
    [TestFixture]
    public class WhenSendingManyIndividualRequestsOfTheSameType : ThroughputSpecificationForBus
    {
        public override async Task SendMessages(IBus bus, Func<bool> shouldKeepSending)
        {
            while (shouldKeepSending())
            {
                var response = await bus.Request(new FooRequest());
                IncrementExpectedMessageCount();
                StressTestMessageHandler.RecordResponseMessageReceipt(response);
            }
        }
    }
}