using System.Threading.Tasks;
using Cafe.Messages.Events;
using Nimbus.InfrastructureContracts.Handlers;
using Waiter.Services;

namespace Waiter.Handlers.WhenAnOrderIsReady
{
    public class CheckWhetherItHasBeenPaidFor : IHandleCompetingEvent<OrderIsReadyEvent>
    {
        private readonly IOrderDeliveryService _orderDeliveryService;

        public CheckWhetherItHasBeenPaidFor(IOrderDeliveryService orderDeliveryService)
        {
            _orderDeliveryService = orderDeliveryService;
        }

        public async Task Handle(OrderIsReadyEvent busEvent)
        {
            _orderDeliveryService.MarkAsMade(busEvent.OrderId);
        }
    }
}