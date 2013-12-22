using System;
using Nimbus.InfrastructureContracts;
using Pizza.Maker.Messages;

namespace Pizza.Ordering.Handlers
{
    public class LetMeKnowWhenMyOrderIsReady : IHandleMulticastEvent<PizzaIsReady>
    {
        public void Handle(PizzaIsReady busEvent)
        {
            Console.WriteLine("Pizza Chef says order {0} is ready", busEvent.PizzaId);
        }
    }
}