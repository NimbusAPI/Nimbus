using System.Threading.Tasks;
using Castle.Windsor;
using Nimbus.Configuration;
using Nimbus.InfrastructureContracts;

namespace Nimbus.UnitTests.MessageBrokerTests.TestInfrastructure.BrokerFactories
{
    public class WindsorCompetingEventBrokerFactory : ICreateMessageBroker<ICompetingEventBroker>
    {
        private IWindsorContainer _container;

        public async Task<ICompetingEventBroker> Create(ITypeProvider typeProvider)
        {
            _container = new WindsorContainer();
            _container.RegisterNimbus(typeProvider);

            return _container.Resolve<ICompetingEventBroker>();
        }

        public void Dispose()
        {
            var container = _container;
            if (container == null) return;
            container.Dispose();
        }
    }
}