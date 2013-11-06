using System;
using System.Threading;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts;

namespace Nimbus.IntegrationTests
{
    public class FakeBroker : ICommandBroker, IMulticastEventBroker, IRequestBroker
    {
        private readonly int _expectedMessages;
        private readonly Semaphore _semaphore = new Semaphore(0, int.MaxValue);
        private int _seenMessages;

        public FakeBroker(int expectedMessages)
        {
            _expectedMessages = expectedMessages;
        }

        public bool AllDone()
        {
            return _seenMessages == _expectedMessages;
        }

        public void WaitUntilDone()
        {
            for (var i = 0; i < _expectedMessages; i++)
            {
                _semaphore.WaitOne();
            }
        }

        public void Dispatch<TBusCommand>(TBusCommand busEvent) where TBusCommand : IBusCommand
        {
            RecordMessageReceipt();
        }

        public void Publish<TBusEvent>(TBusEvent busEvent) where TBusEvent : IBusEvent
        {
            RecordMessageReceipt();
        }

        public TBusResponse Handle<TBusRequest, TBusResponse>(TBusRequest request) where TBusRequest : BusRequest<TBusRequest, TBusResponse> where TBusResponse : IBusResponse
        {
            throw new NotImplementedException();
        }

        private void RecordMessageReceipt()
        {
            Interlocked.Increment(ref _seenMessages);
            _semaphore.Release();

            if (_seenMessages%10 == 0)
            {
                Console.WriteLine("Seen {0} messages", _seenMessages);
            }
        }
    }
}