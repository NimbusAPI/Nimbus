using System;
using System.Threading.Tasks;
using Nimbus.StressTests.ThroughputTests.EventHandlers;
using Nimbus.StressTests.ThroughputTests.MessageContracts;
using NUnit.Framework;

namespace Nimbus.StressTests.ThroughputTests
{
    [TestFixture]
    public class WhenSendingManyRequestsOfTheSameType : ThroughputSpecificationForBus
    {
        public override async Task SendMessages(IBus bus, Func<bool> shouldKeepSending)
        {
            while (shouldKeepSending())
            {
                var response = await bus.Request(new FooRequest(), TimeSpan.FromSeconds(TimeoutSeconds));
                StressTestMessageHandler.RecordResponseMessageReceipt(response);
                IncrementExpectedMessageCount();
            }
        }
    }
}