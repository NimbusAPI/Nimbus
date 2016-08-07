using System;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.PropertyInjection;
using Nimbus.Tests.Common;
using Nimbus.Tests.Common.TestUtilities;
using Nimbus.UnitTests.DispatcherTests.MessageContracts;

#pragma warning disable 4014

namespace Nimbus.UnitTests.DispatcherTests.Handlers
{
    public class BrokerTestCommandHandler : IHandleCommand<FooCommand>, IRequireBusId, IDisposable
    {
        public async Task Handle(FooCommand busCommand)
        {
            MethodCallCounter.ForInstance(BusId).RecordCall<BrokerTestCommandHandler>(h => h.Handle(busCommand));
        }

        public void Dispose()
        {
            MethodCallCounter.ForInstance(BusId).RecordCall<BrokerTestCommandHandler>(h => h.Dispose());
        }

        public Guid BusId { get; set; }
    }
}