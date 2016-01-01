using System.Threading.Tasks;
using Nimbus.Handlers;
using Pizza.Ordering.Messages;

namespace Pizza.Maker.Handlers
{
    public class IncomingOrderHandler : IHandleCommand<OrderPizzaCommand>
    {
        private readonly IPizzaMaker _pizzaMaker;

        public IncomingOrderHandler(IPizzaMaker pizzaMaker)
        {
            _pizzaMaker = pizzaMaker;
        }

        public async Task Handle(OrderPizzaCommand busCommand)
        {
            await _pizzaMaker.MakePizzaForCustomer(busCommand.CustomerName);
        }
    }
}