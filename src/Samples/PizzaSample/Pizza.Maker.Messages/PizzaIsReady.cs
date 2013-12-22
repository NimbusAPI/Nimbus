using Nimbus.MessageContracts;

namespace Pizza.Maker.Messages
{
    public class PizzaIsReady : IBusEvent
    {
        public int PizzaId { get; set; }
         
    }
}