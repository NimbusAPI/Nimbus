using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.SimpleDispatchContextCorrelationTests.MessageContracts;

#pragma warning disable 4014

namespace Nimbus.IntegrationTests.Tests.SimpleDispatchContextCorrelationTests.CommandHandlers
{
    public class FirstCommandHandler : IHandleCommand<FirstCommand>
    {
        internal static IBus Bus;

        public async Task Handle(FirstCommand busCommand)
        {
            Bus.Send(new SecondCommand());
        }
    }
}