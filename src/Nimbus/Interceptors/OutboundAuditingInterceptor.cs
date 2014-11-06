using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Interceptors.Outbound;
using Nimbus.MessageContracts;
using Nimbus.MessageContracts.ControlMessages;
using Nimbus.PropertyInjection;

namespace Nimbus.Interceptors
{
    public class OutboundAuditingInterceptor : OutboundInterceptor, IRequireBus, IRequireClock
    {
        public IBus Bus { get; set; }
        public IClock Clock { get; set; }

        public override async Task OnCommandSent<TBusCommand>(TBusCommand busCommand, BrokeredMessage brokeredMessage)
        {
            var timestamp = Clock.UtcNow;
            var auditEvent = new AuditEvent(busCommand, brokeredMessage.Properties, timestamp);
            await Bus.Publish(auditEvent);
        }

        public override async Task OnRequestSent<TBusRequest, TBusResponse>(IBusRequest<TBusRequest, TBusResponse> busRequest, BrokeredMessage brokeredMessage)
        {
            var timestamp = Clock.UtcNow;
            var auditEvent = new AuditEvent(busRequest, brokeredMessage.Properties, timestamp);
            await Bus.Publish(auditEvent);
        }

        public override async Task OnResponseSent<TBusResponse>(TBusResponse busResponse, BrokeredMessage brokeredMessage)
        {
            var timestamp = Clock.UtcNow;
            var auditEvent = new AuditEvent(busResponse, brokeredMessage.Properties, timestamp);
            await Bus.Publish(auditEvent);
        }

        public override async Task OnMulticastRequestSent<TBusRequest, TBusResponse>(IBusMulticastRequest<TBusRequest, TBusResponse> busRequest, BrokeredMessage brokeredMessage)
        {
            var timestamp = Clock.UtcNow;
            var auditEvent = new AuditEvent(busRequest, brokeredMessage.Properties, timestamp);
            await Bus.Publish(auditEvent);
        }

        public override async Task OnMulticastResponseSent<TBusResponse>(TBusResponse busResponse, BrokeredMessage brokeredMessage)
        {
            var timestamp = Clock.UtcNow;
            var auditEvent = new AuditEvent(busResponse, brokeredMessage.Properties, timestamp);
            await Bus.Publish(auditEvent);
        }

        public override async Task OnEventPublished<TBusEvent>(TBusEvent busEvent, BrokeredMessage brokeredMessage)
        {
            // Quis custodiet ipsos custodes? ;)
            if (typeof (TBusEvent) == typeof (AuditEvent)) return;

            var timestamp = Clock.UtcNow;
            var auditEvent = new AuditEvent(busEvent, brokeredMessage.Properties, timestamp);
            await Bus.Publish(auditEvent);
        }
    }
}