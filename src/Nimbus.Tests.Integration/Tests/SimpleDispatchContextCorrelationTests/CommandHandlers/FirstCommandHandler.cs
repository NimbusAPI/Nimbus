using System;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts;
using Nimbus.InfrastructureContracts.Handlers;
using Nimbus.InfrastructureContracts.PropertyInjection;
using Nimbus.Tests.Common.TestUtilities;
using Nimbus.Tests.Integration.Tests.SimpleDispatchContextCorrelationTests.MessageContracts;

namespace Nimbus.Tests.Integration.Tests.SimpleDispatchContextCorrelationTests.CommandHandlers
{
    public class FirstCommandHandler : IHandleCommand<FirstCommand>, IRequireBus, IRequireBusId
    {
        public IBus Bus { get; set; }
        public Guid BusId { get; set; }
        public IDispatchContext DispatchContext { get; set; }

        public async Task Handle(FirstCommand busCommand)
        {
            MethodCallCounter.ForInstance(BusId).RecordCall<FirstCommandHandler>(ch => ch.Handle(busCommand));

            await Bus.Send(new SecondCommand());
        }
    }
}