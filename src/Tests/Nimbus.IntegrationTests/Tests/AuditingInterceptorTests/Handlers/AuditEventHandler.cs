using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.MessageContracts.ControlMessages;
using Nimbus.Tests.Common;

namespace Nimbus.IntegrationTests.Tests.AuditingInterceptorTests.Handlers
{
#pragma warning disable 4014 // Because this call is not awaited, execution of the current method continues before the call is completed.
	public class AuditEventHandler : IHandleCompetingEvent<AuditEvent>
    {
        public async Task Handle(AuditEvent busEvent)
        {
            MethodCallCounter.RecordCall<AuditEventHandler>(h => h.Handle(busEvent));
            TestHarnessLoggerFactory.Create().Debug("Received audit message {@AuditMessage}", busEvent);
        }
    }
}