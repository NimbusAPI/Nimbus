using Nimbus.InfrastructureContracts;
using Nimbus.IntegrationTests.Tests.SimpleRequestTimeoutTests.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.SimpleRequestTimeoutTests.TimeoutHandlers
{
    public class SomeTimeoutHandler : IHandleTimeouts<SomeTimeout>
    {
        public void Timeout(SomeTimeout busTimeout)
        {
        }
    }
}