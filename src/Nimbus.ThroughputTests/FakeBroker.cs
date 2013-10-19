using System;
using System.Threading;

namespace Nimbus.ThroughputTests
{
    public class FakeBroker : ICommandBroker, IEventBroker, IRequestBroker
    {
        private readonly int _expectedMessages;
        private readonly Semaphore _semaphore = new Semaphore(0, int.MaxValue);

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

        private int _seenMessages;

        public void Dispatch<TBusCommand>(TBusCommand busEvent)
        {
        }

        public void Publish<TBusEvent>(TBusEvent busEvent)
        {
            _seenMessages++;
            _semaphore.Release();

            if (_seenMessages%10 == 0)
            {
                Console.WriteLine("Seen {0} messages", _seenMessages);
            }
        }

        public TBusResponse Handle<TBusRequest, TBusResponse>(TBusRequest request) where TBusRequest : BusRequest<TBusRequest, TBusResponse>
        {
            throw new NotImplementedException();
        }
    }
}