using System;
using Nimbus.InfrastructureContracts;
using Pizza.Maker.Messages;

namespace Pizza.Ordering.Handlers
{
    public class LetMeKnowYouHaveMyOrder : IHandleMulticastEvent<NewOrderRecieved>
    {
        public void Handle(NewOrderRecieved busEvent)
        {
            Console.WriteLine("Pizza Chef says 'Yep, got your order!'");
        }
    }
}