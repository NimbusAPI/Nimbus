using Nimbus.MessageContracts;

namespace Pizza.Maker.Messages
{
    public class NewOrderRecieved : IBusEvent
    {
        public string CustomerName { get; set; }
    }
}