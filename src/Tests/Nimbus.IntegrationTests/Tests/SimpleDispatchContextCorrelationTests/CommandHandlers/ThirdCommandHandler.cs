using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.SimpleDispatchContextCorrelationTests.MessageContracts;
using Nimbus.Tests.Common;

namespace Nimbus.IntegrationTests.Tests.SimpleDispatchContextCorrelationTests.CommandHandlers
{
    public class ThirdCommandHandler : IHandleCommand<ThirdCommand>
    {
        public async Task Handle(ThirdCommand busCommand)
        {
            MethodCallCounter.RecordCall<ThirdCommandHandler>(ch => ch.Handle(busCommand));
        }
    }
}