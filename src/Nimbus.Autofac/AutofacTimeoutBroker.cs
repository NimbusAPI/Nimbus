using Autofac;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts;

namespace Nimbus.Autofac
{
    public class AutofacTimeoutBroker : ITimeoutBroker
    {
        private readonly ILifetimeScope _lifetimeScope;

        public AutofacTimeoutBroker(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        public void Dispatch<TBusTimeout>(TBusTimeout busCommand) where TBusTimeout : IBusTimeout
        {
            using (var scope = _lifetimeScope.BeginLifetimeScope())
            {
                var type = typeof(IHandleTimeouts<TBusTimeout>);

                var handler = (IHandleTimeouts<IBusTimeout>)scope.Resolve(type);
                handler.Timeout(busCommand);
            }
        }
    }
}