using System.Threading.Tasks;
using Castle.Windsor;
using Nimbus.Configuration;
using Nimbus.InfrastructureContracts;

namespace Nimbus.UnitTests.MessageBrokerTests.TestInfrastructure.BrokerFactories
{
    public class WindsorMulticastEventBrokerFactory : ICreateMessageBroker<IMulticastEventBroker>
    {
        private IWindsorContainer _container;

        public async Task<IMulticastEventBroker> Create(ITypeProvider typeProvider)
        {
            _container = new WindsorContainer();
            _container.RegisterNimbus(typeProvider);

            return _container.Resolve<IMulticastEventBroker>();
        }

        public void Dispose()
        {
            var container = _container;
            if (container == null) return;
            container.Dispose();
        }
    }
}