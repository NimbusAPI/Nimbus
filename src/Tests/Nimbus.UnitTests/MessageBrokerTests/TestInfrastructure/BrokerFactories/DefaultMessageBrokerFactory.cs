using System.Threading.Tasks;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;

namespace Nimbus.UnitTests.MessageBrokerTests.TestInfrastructure.BrokerFactories
{
    public class DefaultMessageBrokerFactory : ICreateMessageBroker<ICommandBroker>,
                                               ICreateMessageBroker<IMulticastEventBroker>,
                                               ICreateMessageBroker<ICompetingEventBroker>,
                                               ICreateMessageBroker<IRequestBroker>,
                                               ICreateMessageBroker<IMulticastRequestBroker>
    {
        async Task<ICommandBroker> ICreateMessageBroker<ICommandBroker>.Create(ITypeProvider typeProvider)
        {
            return new DefaultMessageBroker(typeProvider);
        }

        async Task<IMulticastEventBroker> ICreateMessageBroker<IMulticastEventBroker>.Create(ITypeProvider typeProvider)
        {
            return new DefaultMessageBroker(typeProvider);
        }

        async Task<ICompetingEventBroker> ICreateMessageBroker<ICompetingEventBroker>.Create(ITypeProvider typeProvider)
        {
            return new DefaultMessageBroker(typeProvider);
        }

        async Task<IRequestBroker> ICreateMessageBroker<IRequestBroker>.Create(ITypeProvider typeProvider)
        {
            return new DefaultMessageBroker(typeProvider);
        }

        async Task<IMulticastRequestBroker> ICreateMessageBroker<IMulticastRequestBroker>.Create(ITypeProvider typeProvider)
        {
            return new DefaultMessageBroker(typeProvider);
        }

        public void Dispose()
        {
        }
    }
}