using System;
using Nimbus.MessageContracts;

namespace Cafe.Messages
{
    public class PlaceOrderCommand : IBusCommand
    {
        public PlaceOrderCommand()
        {
        }

        public PlaceOrderCommand(Guid orderId, string customerName, string coffeeType)
        {
            OrderId = orderId;
            CustomerName = customerName;
            CoffeeType = coffeeType;
        }

        public Guid OrderId { get; set; }
        public string CustomerName { get; set; }
        public string CoffeeType { get; set; }
    }
}