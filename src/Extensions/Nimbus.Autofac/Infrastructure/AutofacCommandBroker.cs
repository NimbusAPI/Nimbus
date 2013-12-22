using Autofac;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts;

namespace Nimbus.Autofac.Infrastructure
{
    public class AutofacCommandBroker : ICommandBroker
    {
        private readonly ILifetimeScope _lifetimeScope;

        public AutofacCommandBroker(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        public void Dispatch<TBusCommand>(TBusCommand busCommand) where TBusCommand : IBusCommand
        {
            using (var scope = _lifetimeScope.BeginLifetimeScope())
            {
                var type = typeof (IHandleCommand<TBusCommand>);

                var handler = (IHandleCommand<TBusCommand>) scope.Resolve(type);
                handler.Handle(busCommand);
            }
        }
    }
}