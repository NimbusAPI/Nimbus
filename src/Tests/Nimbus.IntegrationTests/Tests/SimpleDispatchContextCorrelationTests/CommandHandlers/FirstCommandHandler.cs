using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.SimpleDispatchContextCorrelationTests.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.SimpleDispatchContextCorrelationTests.CommandHandlers
{
    public class FirstCommandHandler : IHandleCommand<FirstCommand>
    {
        // provided by our test harness setup so that we don't need a container.
        internal static IBus Bus;

        public async Task Handle(FirstCommand busCommand)
        {
            await Bus.Send(new SecondCommand());
        }
    }
}