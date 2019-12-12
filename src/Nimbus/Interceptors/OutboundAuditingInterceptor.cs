using System;
using System.Threading.Tasks;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;
using Nimbus.InfrastructureContracts.PropertyInjection;
using Nimbus.Interceptors.Outbound;
using Nimbus.MessageContracts;
using Nimbus.MessageContracts.ControlMessages;

namespace Nimbus.Interceptors
{
    public class OutboundAuditingInterceptor : OutboundInterceptor, IRequireBus, IRequireDateTime
    {
        public IBus Bus { get; set; }
        public Func<DateTimeOffset> UtcNow { get; set; }

        public override async Task OnCommandSent<TBusCommand>(TBusCommand busCommand, NimbusMessage nimbusMessage)
        {
            var auditEvent = CreateAuditEvent(busCommand, nimbusMessage);
            await Bus.Publish(auditEvent);
        }

        public override async Task OnRequestSent<TBusRequest, TBusResponse>(IBusRequest<TBusRequest, TBusResponse> busRequest, NimbusMessage nimbusMessage)
        {
            var auditEvent = CreateAuditEvent(busRequest, nimbusMessage);
            await Bus.Publish(auditEvent);
        }

        public override async Task OnResponseSent<TBusResponse>(TBusResponse busResponse, NimbusMessage nimbusMessage)
        {
            var auditEvent = CreateAuditEvent(busResponse, nimbusMessage);
            await Bus.Publish(auditEvent);
        }

        public override async Task OnMulticastRequestSent<TBusRequest, TBusResponse>(IBusMulticastRequest<TBusRequest, TBusResponse> busRequest, NimbusMessage nimbusMessage)
        {
            var auditEvent = CreateAuditEvent(busRequest, nimbusMessage);
            await Bus.Publish(auditEvent);
        }

        public override async Task OnMulticastResponseSent<TBusResponse>(TBusResponse busResponse, NimbusMessage nimbusMessage)
        {
            var auditEvent = CreateAuditEvent(busResponse, nimbusMessage);
            await Bus.Publish(auditEvent);
        }

        public override async Task OnEventPublishing<TBusEvent>(TBusEvent busEvent, NimbusMessage nimbusMessage)
        {
            // Quis custodiet ipsos custodes? ;)
            var auditEvent = busEvent as AuditEvent;
            if (auditEvent == null) return;

            nimbusMessage.Properties["AuditedMessageType"] = auditEvent.MessageType;
        }

        public override async Task OnEventPublished<TBusEvent>(TBusEvent busEvent, NimbusMessage nimbusMessage)
        {
            // Quis custodiet ipsos custodes? ;)
            if (busEvent is AuditEvent) return;

            var auditEvent = CreateAuditEvent(busEvent, nimbusMessage);
            await Bus.Publish(auditEvent);
        }

        private AuditEvent CreateAuditEvent(object message, NimbusMessage nimbusMessage)
        {
            var timestamp = UtcNow();
            var auditEvent = new AuditEvent(message.GetType().FullName, message, nimbusMessage.ExtractProperties(), timestamp);
            return auditEvent;
        }
    }
}