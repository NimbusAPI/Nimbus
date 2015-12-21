using System;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.Tests.Common;
using Nimbus.Tests.Common.TestUtilities;
using Nimbus.UnitTests.DispatcherTests.MessageContracts;

#pragma warning disable 4014

namespace Nimbus.UnitTests.DispatcherTests.Handlers
{
    public class BrokerTestCommandHandler : IHandleCommand<FooCommand>, IDisposable
    {
        public async Task Handle(FooCommand busCommand)
        {
            MethodCallCounter.RecordCall<BrokerTestCommandHandler>(h => h.Handle(busCommand));
        }

        public void Dispose()
        {
            MethodCallCounter.RecordCall<BrokerTestCommandHandler>(h => h.Dispose());
        }
    }
}