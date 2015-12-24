using System.Collections.Generic;
using System.Threading.Tasks;
using Nimbus.StressTests.ThroughputTests.EventHandlers;
using Nimbus.StressTests.ThroughputTests.MessageContracts;
using NUnit.Framework;

namespace Nimbus.StressTests.ThroughputTests
{
    [TestFixture]
    public class WhenSendingManyRequestsOfTheSameType : ThroughputSpecificationForBus
    {
        public override async Task SendMessages(IBus bus)
        {
            var batchSize = 100;

            var messageCount = 0;

            while (messageCount < NumMessagesToSend)
            {
                var tasks = new List<Task>();
                for (var i = 0; i < batchSize; i++)
                {
                    if (messageCount >= NumMessagesToSend) break;

                    var task = Task.Run(async () =>
                                              {
                                                  var response = await bus.Request(new FooRequest());
                                                  StressTestMessageHandler.RecordResponseMessageReceipt(response);
                                              });

                    tasks.Add(task);
                    messageCount++;
                }
                await Task.WhenAll(tasks);
            }
        }
    }
}