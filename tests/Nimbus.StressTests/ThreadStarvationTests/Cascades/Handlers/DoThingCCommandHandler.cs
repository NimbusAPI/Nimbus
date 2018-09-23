using System;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.PropertyInjection;
using Nimbus.StressTests.ThreadStarvationTests.Cascades.MessageContracts;
using Nimbus.Tests.Common.TestUtilities;

namespace Nimbus.StressTests.ThreadStarvationTests.Cascades.Handlers
{
    public class DoThingCCommandHandler : IHandleCommand<DoThingCCommand>, IRequireBusId
    {
        public async Task Handle(DoThingCCommand busCommand)
        {
            MethodCallCounter.ForInstance(BusId).RecordCall<DoThingCCommandHandler>(h => h.Handle(busCommand));
        }

        public Guid BusId { get; set; }
    }
}