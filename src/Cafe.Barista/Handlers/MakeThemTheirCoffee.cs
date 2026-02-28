using System;
using System.Threading.Tasks;
using Cafe.Messages.Events;
using Nimbus.InfrastructureContracts;
using Nimbus.InfrastructureContracts.Handlers;
using ILogger = Serilog.ILogger;

namespace Barista.Handlers
{
    public class MakeThemTheirCoffee : IHandleCompetingEvent<OrderPlacedEvent>
    {
        private readonly IBus _bus;
        private readonly ILogger _logger;

        public MakeThemTheirCoffee(IBus bus, ILogger logger)
        {
            _bus = bus;
            _logger = logger;
        }

        public async Task Handle(OrderPlacedEvent busEvent)
        {
            _logger.Debug("{OrderStatus} {Coffee} for {Customer}", "Making", busEvent.CoffeeType, busEvent.CustomerName);
            await Task.Delay(TimeSpan.FromSeconds(1));
            _logger.Information("{OrderStatus} {Coffee} for {Customer}", "Made", busEvent.CoffeeType, busEvent.CustomerName);

            await _bus.Publish(new OrderIsReadyEvent(busEvent.OrderId, busEvent.CoffeeType, busEvent.CustomerName));
        }
    }
}