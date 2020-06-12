using System.Threading.Tasks;
using Cafe.Messages.Events;
using Nimbus.InfrastructureContracts.Handlers;
using Waiter.Services;

namespace Waiter.Handlers.WhenAnOrderIsPaidFor
{
    public class CheckWhetherItHasAlreadyBeenMade : IHandleCompetingEvent<OrderPaidForEvent>
    {
        private readonly IOrderDeliveryService _orderDeliveryService;

        public CheckWhetherItHasAlreadyBeenMade(IOrderDeliveryService orderDeliveryService)
        {
            _orderDeliveryService = orderDeliveryService;
        }

        public async Task Handle(OrderPaidForEvent busEvent)
        {
            _orderDeliveryService.MarkAsPaid(busEvent.OrderId);
        }
    }
}