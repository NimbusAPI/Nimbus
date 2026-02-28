using System;

namespace Waiter.Services
{
    public interface IOrderDeliveryService
    {
        void MarkAsPaid(Guid orderId);
        void MarkAsMade(Guid orderId);
    }
}