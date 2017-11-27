using System;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.UnitTests.TestAssemblies.MessageContracts;

namespace Nimbus.UnitTests.TestAssemblies.Handlers
{
    public class CommandWhoseAssemblyShouldNotBeIncludedHandler : IHandleCommand<CommandWhoseAssemblyShouldNotBeIncluded>
    {
        public async Task Handle(CommandWhoseAssemblyShouldNotBeIncluded busCommand)
        {
            throw new NotImplementedException();
        }
    }
}