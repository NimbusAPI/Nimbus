using System.Threading.Tasks;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;

namespace Nimbus.UnitTests.MessageBrokerTests.TestInfrastructure
{
    public class DefaultMessageBrokerFactory : ICreateMessageBroker<ICommandBroker>
    {
        public async Task<ICommandBroker> Create(ITypeProvider typeProvider)
        {
            return new DefaultMessageBroker(typeProvider);
        }

        public void Dispose()
        {
        }
    }
}