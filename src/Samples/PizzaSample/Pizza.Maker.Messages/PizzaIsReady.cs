using Nimbus.MessageContracts;

namespace Pizza.Maker.Messages
{
    public class PizzaIsReady : IBusEvent
    {
        public string CustomerName { get; set; }
    }
}