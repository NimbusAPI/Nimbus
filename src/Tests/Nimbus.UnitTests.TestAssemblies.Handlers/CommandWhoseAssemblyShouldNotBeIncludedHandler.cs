using System;
using Nimbus.InfrastructureContracts;
using Nimbus.Tests.TestAssemblies.MessageContracts;

namespace Nimbus.Tests.TestAssemblies.Handlers
{
    public class CommandWhoseAssemblyShouldNotBeIncludedHandler : IHandleCommand<CommandWhoseAssemblyShouldNotBeIncluded>
    {
        public void Handle(CommandWhoseAssemblyShouldNotBeIncluded busCommand)
        {
            throw new NotImplementedException();
        }
    }
}