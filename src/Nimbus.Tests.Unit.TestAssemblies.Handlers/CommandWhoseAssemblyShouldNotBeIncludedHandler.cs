using System;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts.Handlers;
using Nimbus.Tests.Unit.TestAssemblies.MessageContracts;

namespace Nimbus.Tests.Unit.TestAssemblies.Handlers
{
    public class CommandWhoseAssemblyShouldNotBeIncludedHandler : IHandleCommand<CommandWhoseAssemblyShouldNotBeIncluded>
    {
        public async Task Handle(CommandWhoseAssemblyShouldNotBeIncluded busCommand)
        {
            throw new NotImplementedException();
        }
    }
}