using Nimbus.MessageContracts;

namespace Pizza.Ordering.Messages
{
    public class OrderPizzaCommand : IBusCommand
    {
        public string CustomerName { get; set; }
    }
}