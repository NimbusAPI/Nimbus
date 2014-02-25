using System;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Pizza.Maker.Messages;

namespace Pizza.Ordering.Handlers
{
    public class LetMeKnowYouHaveMyOrder : IHandleMulticastEvent<NewOrderRecieved>
    {
        public async Task Handle(NewOrderRecieved busEvent)
        {
            Console.WriteLine("Pizza Chef says 'Yep, got your order!'");
        }
    }
}