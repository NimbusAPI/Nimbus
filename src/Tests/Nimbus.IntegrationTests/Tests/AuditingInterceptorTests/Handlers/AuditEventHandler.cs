using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.MessageContracts.ControlMessages;

namespace Nimbus.IntegrationTests.Tests.AuditingInterceptorTests.Handlers
{
    public class AuditEventHandler : IHandleCompetingEvent<AuditEvent>
    {
        public async Task Handle(AuditEvent busEvent)
        {
            MethodCallCounter.RecordCall<AuditEventHandler>(h => h.Handle(busEvent));
        }
    }
}