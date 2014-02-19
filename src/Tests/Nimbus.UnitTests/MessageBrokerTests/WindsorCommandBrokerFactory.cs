using System.Threading.Tasks;
using Castle.Windsor;
using Nimbus.Configuration;
using Nimbus.InfrastructureContracts;

namespace Nimbus.UnitTests.MessageBrokerTests
{
    public class WindsorCommandBrokerFactory : ICreateMessageBroker<ICommandBroker>
    {
        private IWindsorContainer _container;

        public async Task<ICommandBroker> Create(ITypeProvider typeProvider)
        {
            _container = new WindsorContainer();
            _container.RegisterNimbus(typeProvider);

            return _container.Resolve<ICommandBroker>();
        }

        public void Dispose()
        {
            var container = _container;
            if (container == null) return;
            container.Dispose();
        }
    }
}