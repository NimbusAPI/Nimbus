using System.Collections.Generic;
using System.Diagnostics;
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
            const int batchSize = 10;

            var sw = Stopwatch.StartNew();
            while (sw.Elapsed < SendMessagesFor)
            {
                var tasks = new List<Task>();
                for (var i = 0; i < batchSize; i++)
                {
                    var task = Task.Run(async () =>
                                              {
                                                  var response = await bus.Request(new FooRequest());
                                                  StressTestMessageHandler.RecordResponseMessageReceipt(response);
                                              });

                    tasks.Add(task);
                }
                await Task.WhenAll(tasks);

                ExpectToReceiveMessages(batchSize);
            }
        }
    }
}