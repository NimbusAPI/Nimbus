using System;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts.Handlers;
using Nimbus.InfrastructureContracts.PropertyInjection;
using Nimbus.Tests.Common.TestUtilities;
using Nimbus.Tests.Integration.Tests.SimpleCommandSendingTests.MessageContracts;

#pragma warning disable 4014

namespace Nimbus.Tests.Integration.Tests.SimpleCommandSendingTests.CommandHandlers
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