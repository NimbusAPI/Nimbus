using System;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Pizza.Maker.Messages;

namespace Pizza.RetailWeb.ReadModels
{
    public class PizzaOrderStatusHandler : IHandleMulticastEvent<NewOrderRecieved>, IHandleMulticastEvent<PizzaIsReady>
    {
        private readonly PizzaOrderStatusReadModel _readModel;

        public PizzaOrderStatusHandler(PizzaOrderStatusReadModel readModel)
        {
            _readModel = readModel;
        }

        public async Task Handle(NewOrderRecieved busEvent)
        {
            _readModel.AddOrder(busEvent.CustomerName, DateTimeOffset.UtcNow);
        }

        public async Task Handle(PizzaIsReady busEvent)
        {
            _readModel.MarkOrderAsReady(busEvent.CustomerName);
        }
    }
}