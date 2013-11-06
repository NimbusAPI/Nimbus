using System.Collections.Generic;
using Autofac;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts;

namespace Nimbus.Autofac
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
                var handlers = scope.Resolve<IEnumerable<IHandleCommand<TBusCommand>>>();
                
                foreach (var handler in handlers)
                {
                    handler.Handle(busCommand);
                }
            }
        }
    }
}