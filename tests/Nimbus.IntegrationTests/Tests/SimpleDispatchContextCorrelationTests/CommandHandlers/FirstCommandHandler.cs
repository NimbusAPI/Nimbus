using System;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.SimpleDispatchContextCorrelationTests.MessageContracts;
using Nimbus.PropertyInjection;
using Nimbus.Tests.Common.TestUtilities;

namespace Nimbus.IntegrationTests.Tests.SimpleDispatchContextCorrelationTests.CommandHandlers
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