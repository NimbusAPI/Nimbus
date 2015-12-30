using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Nimbus.Extensions;
using Nimbus.Handlers;
using Nimbus.StressTests.ThroughputTests.MessageContracts;
using Serilog;

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

        public static void Reset()
        {
            Messages = new ConcurrentBag<StressTestMessage>();
            ResponseMessages = new ConcurrentBag<StressTestResponseMessage>();
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

        public static int ActualNumMessagesReceived => Messages.Count;

        public static void WaitUntilDone(int expectedNumMessagesReceived, TimeSpan timeout)
        {
            Log.Debug("Waiting until all messages are received.");

            var sw = Stopwatch.StartNew();
            while (true)
            {
                if (sw.Elapsed >= timeout) return;
                if (ActualNumMessagesReceived >= expectedNumMessagesReceived) return;
                Thread.Sleep(TimeSpan.FromMilliseconds(100));
            }
        }

        private static void RecordMessageReceipt(StressTestMessage message)
        {
            message.WhenReceived = DateTimeOffset.UtcNow;

            // Don't block the actual test method. As long as receipt gets recorded sometime soon, we're fine.
            Task.Run(() =>
                     {
                         Messages.Add(message);

                         if (ActualNumMessagesReceived%10 == 0)
                         {
                             Log.Debug("Seen {MessageCount} messages", ActualNumMessagesReceived);
                         }
                     }).ConfigureAwaitFalse();
        }

        public static void RecordResponseMessageReceipt(StressTestResponseMessage message)
        {
            message.WhenReceived = DateTimeOffset.UtcNow;
            Task.Run(() =>
                     {
                         ResponseMessages.Add(message);
                         var responseCount = ResponseMessages.Count;
                         if (responseCount%10 == 0)
                         {
                             Log.Debug("Seen {ResponseCount} responses", responseCount);
                         }
                     }).ConfigureAwaitFalse();
        }
    }
}