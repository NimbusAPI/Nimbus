using Nimbus.MessageContracts;

namespace Pizza.Ordering.Messages
{
    public class HowLongDoPizzasTakeRequest : BusRequest<HowLongDoPizzasTakeRequest, HowLongDoPizzasTakeResponse>
    {
         
    }

    public class HowLongDoPizzasTakeResponse : IBusResponse
    {
        public int Minutes { get; set; }
    }
}