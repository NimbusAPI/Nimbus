using System;
using System.Threading.Tasks;
using Nimbus.IntegrationTests;

namespace Nimbus.ThroughputTests
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var connstring = CommonResources.ConnectionString;

            var messageCount = 50;

            var broker = new FakeBroker(messageCount);

            var bus = new BusBuilder(connstring, broker, broker, broker, new Type[] {}, new Type[] {}, new[] {typeof (MyEvent)}).Build();

            Console.WriteLine("Press any key to start");
            Console.ReadKey();

            bus.Start();

            var startTime = DateTime.Now;

            for (var i = 0; i < messageCount; i++)
            {
                Task.Run(() => bus.Publish(new MyEvent()));
            }

            while (! broker.AllDone())
            {
            }

            var endTime = DateTime.Now;

            var timeTaken = (endTime - startTime);

            Console.WriteLine("All done. Took {0} milliseconds to process {1} messages", timeTaken.TotalMilliseconds, messageCount);
            Console.ReadLine();
        }
    }

    public class FakeBroker : ICommandBroker, IEventBroker, IRequestBroker
    {
        private readonly int _expectedMessages;

        public FakeBroker(int expectedMessages)
        {
            _expectedMessages = expectedMessages;
        }

        public bool AllDone()
        {
            return _seenMessages == _expectedMessages;
        }

        private int _seenMessages;

        public void Dispatch<TBusCommand>(TBusCommand busEvent)
        {
        }

        public void Publish<TBusEvent>(TBusEvent busEvent)
        {
            _seenMessages++;
            if (_seenMessages%10 == 0)
                Console.WriteLine("Seen {0} messages", _seenMessages);
        }

        public TBusResponse Handle<TBusRequest, TBusResponse>(TBusRequest request) where TBusRequest : BusRequest<TBusRequest, TBusResponse>
        {
            throw new NotImplementedException();
        }
    }

    public class MyEvent : IBusEvent
    {
    }
}