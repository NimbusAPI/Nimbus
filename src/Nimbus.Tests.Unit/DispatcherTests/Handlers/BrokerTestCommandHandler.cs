using System;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts.Handlers;
using Nimbus.InfrastructureContracts.PropertyInjection;
using Nimbus.Tests.Common.TestUtilities;
using Nimbus.Tests.Unit.DispatcherTests.MessageContracts;

#pragma warning disable 4014

namespace Nimbus.Tests.Unit.DispatcherTests.Handlers
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