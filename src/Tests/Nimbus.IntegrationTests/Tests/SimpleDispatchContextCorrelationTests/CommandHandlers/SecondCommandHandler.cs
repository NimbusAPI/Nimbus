using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.SimpleDispatchContextCorrelationTests.MessageContracts;
using Nimbus.PropertyInjection;
using Nimbus.Tests.Common;
using Nimbus.Tests.Common.TestUtilities;

namespace Nimbus.IntegrationTests.Tests.SimpleDispatchContextCorrelationTests.CommandHandlers
{
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