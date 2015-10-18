using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.SimpleDispatchContextCorrelationTests.MessageContracts;
using Nimbus.PropertyInjection;
using Nimbus.Tests.Common;

namespace Nimbus.IntegrationTests.Tests.SimpleDispatchContextCorrelationTests.CommandHandlers
{
#pragma warning disable 4014 // Because this call is not awaited, execution of the current method continues before the call is completed.
	public class FirstCommandHandler : IHandleCommand<FirstCommand>, IRequireBus
    {
        public IBus Bus { get; set; }
        public IDispatchContext DispatchContext { get; set; }

        public async Task Handle(FirstCommand busCommand)
        {
            MethodCallCounter.RecordCall<FirstCommandHandler>(ch => ch.Handle(busCommand));

            await Bus.Send(new SecondCommand());
        }
    }
}