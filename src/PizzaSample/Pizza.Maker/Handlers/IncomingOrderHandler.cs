using Nimbus;
using Nimbus.InfrastructureContracts;
using Pizza.Maker.Messages;
using Pizza.Ordering.Messages;

namespace Pizza.Maker.Handlers
{
    public class IncomingOrderHandler : IHandleCommand<OrderPizzaCommand>
    {
        private readonly IBus _bus;
        private readonly IPizzaMaker _pizzaMaker;

        public IncomingOrderHandler(IBus bus, IPizzaMaker pizzaMaker)
        {
            _bus = bus;
            _pizzaMaker = pizzaMaker;
        }

        public void Handle(OrderPizzaCommand busCommand)
        {

            _pizzaMaker.TakePizzaOrder(busCommand.PizzaId);


            _bus.Publish(new NewOrderRecieved {PizzaId = busCommand.PizzaId});

        }
    }
}