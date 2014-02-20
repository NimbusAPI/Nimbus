using System;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts;
using Pizza.Maker.Messages;

namespace Pizza.WaitTimeService.Handlers
{
    public class ListenForNewOrders : IHandleMulticastEvent<NewOrderRecieved>, IHandleMulticastEvent<PizzaIsReady>
    {
        private readonly IWaitTimeCounter _waitTimeCounter;

        public ListenForNewOrders(IWaitTimeCounter waitTimeCounter)
        {
            _waitTimeCounter = waitTimeCounter;
        }

        public async Task Handle(NewOrderRecieved busEvent)
        {
            Console.WriteLine("I heard about a new order");

            _waitTimeCounter.RecordNewPizzaOrder(busEvent.CustomerName);
        }

        public async Task Handle(PizzaIsReady busEvent)
        {
            Console.WriteLine("I heard about a complete order");

            _waitTimeCounter.RecordPizzaCompleted(busEvent.CustomerName);
        }
    }
}