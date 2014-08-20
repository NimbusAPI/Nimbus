using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Interceptors.Outbound;
using Nimbus.MessageContracts;
using Nimbus.MessageContracts.ControlMessages;

namespace Nimbus.Interceptors
{
    public class OutboundAuditingInterceptor : OutboundInterceptor
    {
        private readonly IBus _bus;
        readonly Func<DateTimeOffset> _utcNowFunc;

        public OutboundAuditingInterceptor(IBus bus, Func<DateTimeOffset> utcNowFunc)
        {
            _bus = bus;
            _utcNowFunc = utcNowFunc;
        }

        public override async Task OnCommandSending<TBusCommand>(TBusCommand busCommand, BrokeredMessage brokeredMessage)
        {
            var timestamp = _utcNowFunc();
            var auditEvent = new AuditEvent(busCommand, brokeredMessage.Properties, timestamp);
            await _bus.Publish(auditEvent);
        }

        public override async Task OnRequestSending<TBusRequest, TBusResponse>(IBusRequest<TBusRequest, TBusResponse> busRequest, BrokeredMessage brokeredMessage)
        {
            var timestamp = _utcNowFunc();
            var auditEvent = new AuditEvent(busRequest, brokeredMessage.Properties, timestamp);
            await _bus.Publish(auditEvent);
        }

        public override async Task OnResponseSending<TBusResponse>(TBusResponse busResponse, BrokeredMessage brokeredMessage)
        {
            var timestamp = _utcNowFunc();
            var auditEvent = new AuditEvent(busResponse, brokeredMessage.Properties, timestamp);
            await _bus.Publish(auditEvent);
        }

        public override async Task OnMulticastRequestSending<TBusRequest, TBusResponse>(IBusMulticastRequest<TBusRequest, TBusResponse> busRequest, BrokeredMessage brokeredMessage)
        {
            var timestamp = _utcNowFunc();
            var auditEvent = new AuditEvent(busRequest, brokeredMessage.Properties, timestamp);
            await _bus.Publish(auditEvent);
        }

        public override async Task OnMulticastResponseSending<TBusResponse>(TBusResponse busResponse, BrokeredMessage brokeredMessage)
        {
            var timestamp = _utcNowFunc();
            var auditEvent = new AuditEvent(busResponse, brokeredMessage.Properties, timestamp);
            await _bus.Publish(auditEvent);
        }

        public override async Task OnEventPublishing<TBusEvent>(TBusEvent busEvent, BrokeredMessage brokeredMessage)
        {
            // Quis custodiet ipsos custodes? ;)
            if (typeof(TBusEvent) == typeof(AuditEvent)) return;

            var timestamp = _utcNowFunc();
            var auditEvent = new AuditEvent(busEvent, brokeredMessage.Properties, timestamp);
            await _bus.Publish(auditEvent);
        }
    }
}