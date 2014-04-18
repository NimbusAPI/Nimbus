using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Pizza.RetailWeb.Models.Home;

namespace Pizza.RetailWeb.ReadModels
{
    public class PizzaOrderStatusReadModel : IAmAReadModel
    {
        private readonly ConcurrentDictionary<string, PizzaOrderStatus> _orders = new ConcurrentDictionary<string, PizzaOrderStatus>();

        public IEnumerable<PizzaOrderStatus> Orders
        {
            get { return _orders.Values; }
        }

        public void AddOrder(string customerName, DateTimeOffset timestamp)
        {
            var orderStatus = new PizzaOrderStatus
                              {
                                  CustomerName = customerName,
                                  Ordered = timestamp,
                              };
            _orders[orderStatus.CustomerName] = orderStatus;
        }

        public void MarkOrderAsReady(string customerName)
        {
            PizzaOrderStatus orderStatus;
            if (!_orders.TryGetValue(customerName, out orderStatus)) return;

            orderStatus.Ready = DateTimeOffset.UtcNow;
        }
    }
}