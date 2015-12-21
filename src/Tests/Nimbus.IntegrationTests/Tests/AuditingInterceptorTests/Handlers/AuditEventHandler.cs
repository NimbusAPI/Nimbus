using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.MessageContracts.ControlMessages;
using Nimbus.Tests.Common;
using Nimbus.Tests.Common.Stubs;
using Nimbus.Tests.Common.TestUtilities;

namespace Nimbus.IntegrationTests.Tests.AuditingInterceptorTests.Handlers
{
    public class AuditEventHandler : IHandleCompetingEvent<AuditEvent>
    {
        public async Task Handle(AuditEvent busEvent)
        {
            MethodCallCounter.RecordCall<AuditEventHandler>(h => h.Handle(busEvent));
            TestHarnessLoggerFactory.Create().Debug("Received audit message {@AuditMessage}", busEvent);
        }
    }
}