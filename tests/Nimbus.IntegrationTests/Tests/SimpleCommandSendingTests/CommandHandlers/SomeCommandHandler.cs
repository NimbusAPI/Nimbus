using System;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.SimpleCommandSendingTests.MessageContracts;
using Nimbus.PropertyInjection;
using Nimbus.Tests.Common;
using Nimbus.Tests.Common.TestUtilities;

#pragma warning disable 4014

namespace Nimbus.IntegrationTests.Tests.SimpleCommandSendingTests.CommandHandlers
{
    public class SomeCommandHandler : IHandleCommand<SomeCommand>, IRequireBusId
    {
        public async Task Handle(SomeCommand busCommand)
        {
            MethodCallCounter.ForInstance(BusId).RecordCall<SomeCommandHandler>(ch => ch.Handle(busCommand));
        }

        public Guid BusId { get; set; }
    }
}