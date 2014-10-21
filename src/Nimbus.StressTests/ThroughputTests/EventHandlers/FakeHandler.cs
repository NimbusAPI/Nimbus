using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.StressTests.ThroughputTests.MessageContracts;

namespace Nimbus.StressTests.ThroughputTests.EventHandlers
{
    /// <summary>
    ///     This class needs to exist so that our bus knows to subscribe to these types of message. That's all. It's not
    ///     supposed to do anything.
    /// </summary>
    public class FakeHandler : IHandleMulticastEvent<FooEvent>,
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
        private readonly int _expectedNumMessagesReceived;
        private int _actualNumMessagesReceived;

        public FakeHandler(int expectedNumMessagesReceived)
        {
            _expectedNumMessagesReceived = expectedNumMessagesReceived;
        }

        public int ActualNumMessagesReceived
        {
            get { return _actualNumMessagesReceived; }
        }

        public int ExpectedNumMessagesReceived
        {
            get { return _expectedNumMessagesReceived; }
        }

        public void WaitUntilDone(TimeSpan timeout)
        {
            var sw = Stopwatch.StartNew();
            while (true)
            {
                if (sw.Elapsed >= timeout) return;
                if (_actualNumMessagesReceived >= ExpectedNumMessagesReceived) return;
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }

        public async Task Handle(FooEvent busEvent)
        {
            RecordMessageReceipt();
        }

        public async Task Handle(BarEvent busEvent)
        {
            RecordMessageReceipt();
        }

        public async Task Handle(BazEvent busEvent)
        {
            RecordMessageReceipt();
        }

        public async Task Handle(QuxEvent busEvent)
        {
            RecordMessageReceipt();
        }

        public async Task Handle(FooCommand busCommand)
        {
            RecordMessageReceipt();
        }

        public async Task Handle(BarCommand busCommand)
        {
            RecordMessageReceipt();
        }

        public async Task Handle(BazCommand busCommand)
        {
            RecordMessageReceipt();
        }

        public async Task Handle(QuxCommand busCommand)
        {
            RecordMessageReceipt();
        }

        public async Task<FooResponse> Handle(FooRequest busRequest)
        {
            RecordMessageReceipt();
            return new FooResponse();
        }

        private void RecordMessageReceipt()
        {
            Interlocked.Increment(ref _actualNumMessagesReceived);

            if (_actualNumMessagesReceived%10 == 0)
            {
                Console.WriteLine("Seen {0} messages", _actualNumMessagesReceived);
            }
        }
    }
}