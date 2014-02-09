using System;
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

        public void Handle(NewOrderRecieved busEvent)
        {
            Console.WriteLine("I heard about a new order");

            _waitTimeCounter.RecordNewPizzaOrder(busEvent.PizzaId);
        }

        public void Handle(PizzaIsReady busEvent)
        {
            Console.WriteLine("I heard about a complete order");

            _waitTimeCounter.RecordPizzaCompleted(busEvent.PizzaId);
        }
    }
}