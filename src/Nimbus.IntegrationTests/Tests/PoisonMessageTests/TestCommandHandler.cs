using System;
using Nimbus.InfrastructureContracts;

namespace Nimbus.IntegrationTests.Tests.PoisonMessageTests
{
    public class TestCommandHandler : IHandleCommand<TestCommand>
    {
        public void Handle(TestCommand busCommand)
        {
            throw new Exception("This handler is supposed to fail.");
        }
    }
}