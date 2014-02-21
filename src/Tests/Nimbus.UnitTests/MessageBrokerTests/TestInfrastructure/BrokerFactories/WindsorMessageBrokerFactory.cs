using System;
using System.Threading.Tasks;
using Castle.Windsor;
using Nimbus.Configuration;
using Nimbus.InfrastructureContracts;

namespace Nimbus.UnitTests.MessageBrokerTests.TestInfrastructure.BrokerFactories
{
    public class WindsorMessageBrokerFactory : ICreateMessageHandlerFactory<ICommandHandlerFactory>,
                                               ICreateMessageHandlerFactory<IMulticastEventHandlerFactory>,
                                               ICreateMessageHandlerFactory<ICompetingEventHandlerFactory>,
                                               ICreateMessageHandlerFactory<IRequestHandlerFactory>,
                                               ICreateMessageHandlerFactory<IMulticastRequestHandlerFactory>
    {
        private IWindsorContainer _container;

        async Task<IMulticastEventHandlerFactory> ICreateMessageHandlerFactory<IMulticastEventHandlerFactory>.Create(ITypeProvider typeProvider)
        {
            BuildContainer(typeProvider);

            return _container.Resolve<IMulticastEventHandlerFactory>();
        }

        async Task<ICompetingEventHandlerFactory> ICreateMessageHandlerFactory<ICompetingEventHandlerFactory>.Create(ITypeProvider typeProvider)
        {
            BuildContainer(typeProvider);

            return _container.Resolve<ICompetingEventHandlerFactory>();
        }

        async Task<ICommandHandlerFactory> ICreateMessageHandlerFactory<ICommandHandlerFactory>.Create(ITypeProvider typeProvider)
        {
            BuildContainer(typeProvider);

            return _container.Resolve<ICommandHandlerFactory>();
        }

        async Task<IRequestHandlerFactory> ICreateMessageHandlerFactory<IRequestHandlerFactory>.Create(ITypeProvider typeProvider)
        {
            BuildContainer(typeProvider);

            return _container.Resolve<IRequestHandlerFactory>();
        }

        async Task<IMulticastRequestHandlerFactory> ICreateMessageHandlerFactory<IMulticastRequestHandlerFactory>.Create(ITypeProvider typeProvider)
        {
            BuildContainer(typeProvider);

            return _container.Resolve<IMulticastRequestHandlerFactory>();
        }

        private void BuildContainer(ITypeProvider typeProvider)
        {
            if (_container != null) throw new InvalidOperationException("This factory is only supposed to be used to construct one test subject.");

            _container = new WindsorContainer();
            _container.RegisterNimbus(typeProvider);
        }

        public void Dispose()
        {
            var container = _container;
            if (container == null) return;
            container.Dispose();
        }
    }
}