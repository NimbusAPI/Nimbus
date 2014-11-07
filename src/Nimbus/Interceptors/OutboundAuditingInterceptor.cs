using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Interceptors.Outbound;
using Nimbus.MessageContracts;
using Nimbus.MessageContracts.ControlMessages;
using Nimbus.PropertyInjection;

namespace Nimbus.Interceptors
{
    public class OutboundAuditingInterceptor : OutboundInterceptor, IRequireBus, IRequireDateTime
    {
        public IBus Bus { get; set; }
        public Func<DateTimeOffset> UtcNow { get; set; }

        public override async Task OnCommandSent<TBusCommand>(TBusCommand busCommand, BrokeredMessage brokeredMessage)
        {
            var timestamp = UtcNow();
            var auditEvent = new AuditEvent(busCommand.GetType().FullName, busCommand, brokeredMessage.Properties, timestamp);
            await Bus.Publish(auditEvent);
        }

        public override async Task OnRequestSent<TBusRequest, TBusResponse>(IBusRequest<TBusRequest, TBusResponse> busRequest, BrokeredMessage brokeredMessage)
        {
            var timestamp = UtcNow();
            var auditEvent = new AuditEvent(busRequest.GetType().FullName, busRequest, brokeredMessage.Properties, timestamp);
            await Bus.Publish(auditEvent);
        }

        public override async Task OnResponseSent<TBusResponse>(TBusResponse busResponse, BrokeredMessage brokeredMessage)
        {
            var timestamp = UtcNow();
            var auditEvent = new AuditEvent(busResponse.GetType().FullName, busResponse, brokeredMessage.Properties, timestamp);
            await Bus.Publish(auditEvent);
        }

        public override async Task OnMulticastRequestSent<TBusRequest, TBusResponse>(IBusMulticastRequest<TBusRequest, TBusResponse> busRequest, BrokeredMessage brokeredMessage)
        {
            var timestamp = UtcNow();
            var auditEvent = new AuditEvent(busRequest.GetType().FullName, busRequest, brokeredMessage.Properties, timestamp);
            await Bus.Publish(auditEvent);
        }

        public override async Task OnMulticastResponseSent<TBusResponse>(TBusResponse busResponse, BrokeredMessage brokeredMessage)
        {
            var timestamp = UtcNow();
            var auditEvent = new AuditEvent(busResponse.GetType().FullName, busResponse, brokeredMessage.Properties, timestamp);
            await Bus.Publish(auditEvent);
        }

        public override async Task OnEventPublishing<TBusEvent>(TBusEvent busEvent, BrokeredMessage brokeredMessage)
        {
            // Quis custodiet ipsos custodes? ;)
            var auditEvent = busEvent as AuditEvent;
            if (auditEvent == null) return;

            brokeredMessage.Properties["AuditedMessageType"] = auditEvent.MessageType;
        }

        public override async Task OnEventPublished<TBusEvent>(TBusEvent busEvent, BrokeredMessage brokeredMessage)
        {
            // Quis custodiet ipsos custodes? ;)
            if (busEvent is AuditEvent) return;

            var timestamp = UtcNow();
            var auditEvent = new AuditEvent(busEvent.GetType().FullName, busEvent, brokeredMessage.Properties, timestamp);
            await Bus.Publish(auditEvent);
        }
    }
}