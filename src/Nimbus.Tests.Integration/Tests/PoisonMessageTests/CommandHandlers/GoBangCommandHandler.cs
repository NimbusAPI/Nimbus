using System;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts.Handlers;
using Nimbus.InfrastructureContracts.PropertyInjection;
using Nimbus.Tests.Common.TestUtilities;
using Nimbus.Tests.Integration.Tests.PoisonMessageTests.MessageContracts;

namespace Nimbus.Tests.Integration.Tests.PoisonMessageTests.CommandHandlers
{
    public class GoBangCommandHandler : IHandleCommand<GoBangCommand>, IRequireBusId
    {
        public async Task Handle(GoBangCommand busCommand)
        {
            MethodCallCounter.ForInstance(BusId).RecordCall<GoBangCommandHandler>(h => h.Handle(busCommand));

            throw new Exception("This handler is supposed to fail.");
        }

        public Guid BusId { get; set; }
    }
}