using System;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.PoisonMessageTests.MessageContracts;
using Nimbus.PropertyInjection;
using Nimbus.Tests.Common;
using Nimbus.Tests.Common.TestUtilities;

namespace Nimbus.IntegrationTests.Tests.PoisonMessageTests.CommandHandlers
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