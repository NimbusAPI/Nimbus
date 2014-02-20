using System;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts;
using Pizza.Maker.Messages;

namespace Pizza.Ordering.Handlers
{
    public class LetMeKnowWhenMyOrderIsReady : IHandleMulticastEvent<PizzaIsReady>
    {
        public async Task Handle(PizzaIsReady busEvent)
        {
            Console.WriteLine("Pizza Chef says, '{0}! Your order is ready!'", busEvent.CustomerName);
        }
    }
}