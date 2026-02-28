using System;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts.Handlers;
using Nimbus.InfrastructureContracts.PropertyInjection;
using Nimbus.Tests.Common.TestUtilities;
using Nimbus.Tests.Integration.Tests.SimpleDispatchContextCorrelationTests.MessageContracts;

namespace Nimbus.Tests.Integration.Tests.SimpleDispatchContextCorrelationTests.CommandHandlers
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