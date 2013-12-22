using Nimbus.MessageContracts;

namespace Pizza.Maker.Messages
{
    public class NewOrderRecieved : IBusEvent
    {
        public int PizzaId { get; set; }
         
    }
}