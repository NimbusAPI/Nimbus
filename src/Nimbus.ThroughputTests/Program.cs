using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nimbus.IntegrationTests;

namespace Nimbus.ThroughputTests
{
    class Program
    {
        static void Main(string[] args)
        {

            var connstring = CommonResources.ConnectionString;

            var messageCount = 50;


            var broker = new FakeBroker(messageCount);

            var bus = new Bus(connstring, new QueueManager(connstring), broker, broker, broker, new Type[] {},
                              new Type[] {}, new Type[] {typeof (MyEvent)});


            Console.WriteLine("Press any key to start");

            Console.ReadKey();

            bus.Start();


            var startTime = DateTime.Now;

            for (int i = 0; i < messageCount; i++)
            {
                Task.Run(() => bus.Publish(new MyEvent()));
            }

            while (! broker.AllDone())
            {
            }

            var endTime = DateTime.Now;

            var timeTaken = (TimeSpan) (endTime - startTime);

            Console.WriteLine("All done, took {0} milliseconds to process {1} messages", timeTaken.TotalMilliseconds, messageCount);

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

        private int _seenMessages = 0;

        public void Dispatch<TBusCommand>(TBusCommand busEvent)
        {
        }

        public void Publish<TBusEvent>(TBusEvent busEvent)
        {

            _seenMessages++;
            if (_seenMessages % 10 == 0)
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
