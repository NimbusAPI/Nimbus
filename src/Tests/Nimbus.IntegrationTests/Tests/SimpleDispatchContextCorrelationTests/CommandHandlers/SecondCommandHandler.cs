using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.SimpleDispatchContextCorrelationTests.MessageContracts;
using Nimbus.PropertyInjection;
using Nimbus.Tests.Common;

namespace Nimbus.IntegrationTests.Tests.SimpleDispatchContextCorrelationTests.CommandHandlers
{
#pragma warning disable 4014 // Because this call is not awaited, execution of the current method continues before the call is completed.
	public class SecondCommandHandler : IHandleCommand<SecondCommand>, IRequireBus
    {
        public IBus Bus { get; set; }

        public async Task Handle(SecondCommand busCommand)
        {
            MethodCallCounter.RecordCall<SecondCommandHandler>(ch => ch.Handle(busCommand));
            await Bus.Send(new ThirdCommand());
        }
    }
}