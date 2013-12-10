using Nimbus.MessageContracts;

namespace Pizza.Ordering.Messages
{
    public class OrderPizzaCommand : IBusCommand
    {
        public int PizzaId { get; set; }
         
    }
}