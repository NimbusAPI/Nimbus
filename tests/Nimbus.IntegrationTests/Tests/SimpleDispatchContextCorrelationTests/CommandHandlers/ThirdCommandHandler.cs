using System;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.SimpleDispatchContextCorrelationTests.MessageContracts;
using Nimbus.PropertyInjection;
using Nimbus.Tests.Common;
using Nimbus.Tests.Common.TestUtilities;

namespace Nimbus.IntegrationTests.Tests.SimpleDispatchContextCorrelationTests.CommandHandlers
{
    public class ThirdCommandHandler : IHandleCommand<ThirdCommand>, IRequireBusId
    {
        public async Task Handle(ThirdCommand busCommand)
        {
            MethodCallCounter.ForInstance(BusId).RecordCall<ThirdCommandHandler>(ch => ch.Handle(busCommand));
        }

        public Guid BusId { get; set; }
    }
}