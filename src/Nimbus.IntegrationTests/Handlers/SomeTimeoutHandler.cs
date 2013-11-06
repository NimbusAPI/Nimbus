using Nimbus.InfrastructureContracts;
using Nimbus.IntegrationTests.MessageContracts;

namespace Nimbus.IntegrationTests.Handlers
{
    public class SomeTimeoutHandler: IHandleTimeout<SomeTimeout>
    {
        public void Timeout(SomeTimeout busTimeout)
        {
            throw new System.NotImplementedException();
        }
    }
}