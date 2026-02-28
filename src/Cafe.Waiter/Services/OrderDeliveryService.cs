using System;
using System.Collections.Generic;
using Serilog;

namespace Waiter.Services
{
    public class OrderDeliveryService : IOrderDeliveryService
    {
        private readonly ILogger _logger;
        private readonly List<Guid> _madeOrderIds = new List<Guid>();
        private readonly List<Guid> _paidOrderIds = new List<Guid>();

        public OrderDeliveryService(ILogger logger)
        {
            _logger = logger;
        }

        public void MarkAsPaid(Guid orderId)
        {
            _paidOrderIds.Add(orderId);
            CheckWhetherWeShouldDeliver(orderId);
        }

        public void MarkAsMade(Guid orderId)
        {
            _madeOrderIds.Add(orderId);
            CheckWhetherWeShouldDeliver(orderId);
        }

        public bool HasBeenPaid(Guid orderId)
        {
            var hasBeenPaid = _paidOrderIds.Contains(orderId);
            return hasBeenPaid;
        }

        private void CheckWhetherWeShouldDeliver(Guid orderId)
        {
            if (!_madeOrderIds.Contains(orderId))
            {
                _logger.Information("{OrderId} isn't ready yet. We can't give it to the customer.", orderId);
                return;
            }

            if (!_paidOrderIds.Contains(orderId))
            {
                _logger.Information("{OrderId} hasn't been paid for yet. We can't give it to the customer.", orderId);
                return;
            }

            DeliverOrderToCustomer(orderId);
        }

        private void DeliverOrderToCustomer(Guid orderId)
        {
            _logger.Information("Delivering {OrderId} to the customer.", orderId);
        }
    }
}