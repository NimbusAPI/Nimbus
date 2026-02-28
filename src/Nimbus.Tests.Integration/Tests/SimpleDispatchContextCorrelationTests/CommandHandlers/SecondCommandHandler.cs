using System;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts;
using Nimbus.InfrastructureContracts.Handlers;
using Nimbus.InfrastructureContracts.PropertyInjection;
using Nimbus.Tests.Common.TestUtilities;
using Nimbus.Tests.Integration.Tests.SimpleDispatchContextCorrelationTests.MessageContracts;

namespace Nimbus.Tests.Integration.Tests.SimpleDispatchContextCorrelationTests.CommandHandlers
{
    public class SecondCommandHandler : IHandleCommand<SecondCommand>, IRequireBus, IRequireBusId
    {
        public IBus Bus { get; set; }
        public Guid BusId { get; set; }

        public async Task Handle(SecondCommand busCommand)
        {
            MethodCallCounter.ForInstance(BusId).RecordCall<SecondCommandHandler>(ch => ch.Handle(busCommand));
            await Bus.Send(new ThirdCommand());
        }
    }
}