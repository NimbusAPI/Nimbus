using System;
using Nimbus.InfrastructureContracts;
using Nimbus.UnitTests.TestAssemblies.MessageContracts;

namespace Nimbus.UnitTests.TestAssemblies.Handlers
{
    public class CommandWhoseAssemblyShouldNotBeIncludedHandler : IHandleCommand<CommandWhoseAssemblyShouldNotBeIncluded>
    {
        public void Handle(CommandWhoseAssemblyShouldNotBeIncluded busCommand)
        {
            throw new NotImplementedException();
        }
    }
}