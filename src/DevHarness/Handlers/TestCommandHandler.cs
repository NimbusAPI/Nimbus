using System;
using System.Threading.Tasks;
using DevHarness.Messages;
using Nimbus.InfrastructureContracts.Handlers;

namespace DevHarness.Handlers
{
    public class TestCommandHandler : IHandleCommand<TestCommand>
    {
        public async Task Handle(TestCommand busCommand)
        {
            Console.WriteLine(busCommand.Name);
        }
    }
}