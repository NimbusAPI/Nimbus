using System;
using System.Threading.Tasks;
using Nimbus.MessageContracts;

namespace Nimbus.Infrastructure.Events
{
    internal interface IEventSender
    {
        Task Publish<TBusEvent>(TBusEvent busEvent) where TBusEvent : IBusEvent;
        Task PublishAt<TBusEvent>(TBusEvent busEvent, DateTimeOffset deliveryTime) where TBusEvent : IBusEvent;
    }
}