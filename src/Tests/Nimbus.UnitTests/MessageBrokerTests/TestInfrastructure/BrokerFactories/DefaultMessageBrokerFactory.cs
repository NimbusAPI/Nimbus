using System.Threading.Tasks;
using Nimbus.HandlerFactories;
using Nimbus.Infrastructure;

namespace Nimbus.UnitTests.MessageBrokerTests.TestInfrastructure.BrokerFactories
{
    //FIXME rename
    public class DefaultMessageBrokerFactory : ICreateMessageHandlerFactory<ICommandHandlerFactory>,
                                               ICreateMessageHandlerFactory<IMulticastEventHandlerFactory>,
                                               ICreateMessageHandlerFactory<ICompetingEventHandlerFactory>,
                                               ICreateMessageHandlerFactory<IRequestHandlerFactory>,
                                               ICreateMessageHandlerFactory<IMulticastRequestHandlerFactory>
    {
        async Task<ICommandHandlerFactory> ICreateMessageHandlerFactory<ICommandHandlerFactory>.Create(ITypeProvider typeProvider)
        {
            return new DefaultMessageHandlerFactory(typeProvider);
        }

        async Task<IMulticastEventHandlerFactory> ICreateMessageHandlerFactory<IMulticastEventHandlerFactory>.Create(ITypeProvider typeProvider)
        {
            return new DefaultMessageHandlerFactory(typeProvider);
        }

        async Task<ICompetingEventHandlerFactory> ICreateMessageHandlerFactory<ICompetingEventHandlerFactory>.Create(ITypeProvider typeProvider)
        {
            return new DefaultMessageHandlerFactory(typeProvider);
        }

        async Task<IRequestHandlerFactory> ICreateMessageHandlerFactory<IRequestHandlerFactory>.Create(ITypeProvider typeProvider)
        {
            return new DefaultMessageHandlerFactory(typeProvider);
        }

        async Task<IMulticastRequestHandlerFactory> ICreateMessageHandlerFactory<IMulticastRequestHandlerFactory>.Create(ITypeProvider typeProvider)
        {
            return new DefaultMessageHandlerFactory(typeProvider);
        }

        public void Dispose()
        {
        }
    }
}