using Nimbus.MessageContracts;

namespace Pizza.Ordering.Messages
{
    public class HowLongDoPizzasTakeResponse : IBusResponse
    {
        public int Minutes { get; set; }
    }
}