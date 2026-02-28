using System.Threading.Tasks;
using Cafe.Messages;
using Cafe.Messages.Events;
using Nimbus.InfrastructureContracts;
using Nimbus.InfrastructureContracts.Handlers;
using ILogger = Serilog.ILogger;

namespace Cashier.Handlers
{
    public class PlaceOrderCommandHandler : IHandleCommand<PlaceOrderCommand>
    {
        private readonly IBus _bus;
        private readonly ILogger _logger;

        public PlaceOrderCommandHandler(IBus bus, ILogger logger)
        {
            _bus = bus;
            _logger = logger;
        }

        public async Task Handle(PlaceOrderCommand busCommand)
        {
            // for now, we'll pretend that we take the customer's money before actually adding the order to the queue
            _logger.Information("{Customer} just paid for their coffee. Thank you :)", busCommand.CustomerName);
            await _bus.Publish(new OrderPaidForEvent(busCommand.OrderId));

            _logger.Information("{Customer} would like a {CoffeeType}", busCommand.CustomerName, busCommand.CoffeeType);
            await _bus.Publish(new OrderPlacedEvent(busCommand.OrderId, busCommand.CoffeeType, busCommand.CustomerName));
        }
    }
}