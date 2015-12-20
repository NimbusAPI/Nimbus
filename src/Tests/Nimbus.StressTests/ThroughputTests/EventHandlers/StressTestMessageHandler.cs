using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.StressTests.ThroughputTests.MessageContracts;

namespace Nimbus.StressTests.ThroughputTests.EventHandlers
{
    public class StressTestMessageHandler : IHandleMulticastEvent<FooEvent>,
                                            IHandleMulticastEvent<BarEvent>,
                                            IHandleMulticastEvent<BazEvent>,
                                            IHandleMulticastEvent<QuxEvent>,
                                            IHandleCompetingEvent<FooEvent>,
                                            IHandleCompetingEvent<BarEvent>,
                                            IHandleCompetingEvent<BazEvent>,
                                            IHandleCompetingEvent<QuxEvent>,
                                            IHandleCommand<FooCommand>,
                                            IHandleCommand<BarCommand>,
                                            IHandleCommand<BazCommand>,
                                            IHandleCommand<QuxCommand>,
                                            IHandleRequest<FooRequest, FooResponse>
    {
        public static ConcurrentBag<StressTestMessage> Messages { get; private set; } = new ConcurrentBag<StressTestMessage>();
        public static ConcurrentBag<StressTestResponseMessage> ResponseMessages { get; private set; } = new ConcurrentBag<StressTestResponseMessage>();

        public static void Reset(int expectedNumMessagesReceived)
        {
            Messages = new ConcurrentBag<StressTestMessage>();
            ResponseMessages = new ConcurrentBag<StressTestResponseMessage>();
            ExpectedNumMessagesReceived = expectedNumMessagesReceived;
        }

        public async Task Handle(FooEvent busEvent)
        {
            RecordMessageReceipt(busEvent);
        }

        public async Task Handle(BarEvent busEvent)
        {
            RecordMessageReceipt(busEvent);
        }

        public async Task Handle(BazEvent busEvent)
        {
            RecordMessageReceipt(busEvent);
        }

        public async Task Handle(QuxEvent busEvent)
        {
            RecordMessageReceipt(busEvent);
        }

        public async Task Handle(FooCommand busCommand)
        {
            RecordMessageReceipt(busCommand);
        }

        public async Task Handle(BarCommand busCommand)
        {
            RecordMessageReceipt(busCommand);
        }

        public async Task Handle(BazCommand busCommand)
        {
            RecordMessageReceipt(busCommand);
        }

        public async Task Handle(QuxCommand busCommand)
        {
            RecordMessageReceipt(busCommand);
        }

        public async Task<FooResponse> Handle(FooRequest busRequest)
        {
            RecordMessageReceipt(busRequest);

            return new FooResponse
                   {
                       RequestSentAt = busRequest.WhenSent
                   };
        }

        public static int ExpectedNumMessagesReceived { get; private set; }
        public static int ActualNumMessagesReceived => Messages.Count;

        public static void WaitUntilDone(TimeSpan timeout)
        {
            var sw = Stopwatch.StartNew();
            while (true)
            {
                if (sw.Elapsed >= timeout) return;
                if (ActualNumMessagesReceived >= ExpectedNumMessagesReceived) return;
                Thread.Sleep(TimeSpan.FromMilliseconds(100));
            }
        }

        private static void RecordMessageReceipt(StressTestMessage message)
        {
            message.WhenReceived = DateTimeOffset.UtcNow;
            Messages.Add(message);

            if (ActualNumMessagesReceived%10 == 0)
            {
                Console.WriteLine("Seen {0} messages", ActualNumMessagesReceived);
            }
        }

        public static void RecordResponseMessageReceipt(StressTestResponseMessage message)
        {
            message.WhenReceived = DateTimeOffset.UtcNow;
            ResponseMessages.Add(message);
        }
    }
}