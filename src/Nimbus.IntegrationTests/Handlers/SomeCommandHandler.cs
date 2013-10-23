using Nimbus.InfrastructureContracts;
using Nimbus.IntegrationTests.MessageContracts;

namespace Nimbus.IntegrationTests.Handlers
{
    public class SomeCommandHandler: IHandleCommand<SomeCommand>
    {
        public void Handle(SomeCommand busCommand)
        {
            throw new System.NotImplementedException();
        }
    }
}