using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts.Handlers;
using Nimbus.MessageContracts;
using Shouldly;

namespace Nimbus.Tests.Unit.ConfigurationTests
{
    public class WhenScanningAssembliesForHandledCommandTypes : SpecificationFor<AssemblyScanningTypeProvider>
    {
        private Type[] _commandHandlerTypes;

        protected override AssemblyScanningTypeProvider Given()
        {
            return new AssemblyScanningTypeProvider(typeof (WhenScanningAssembliesForHandledCommandTypes).Assembly);
        }

        protected override void When()
        {
            _commandHandlerTypes = Subject.CommandHandlerTypes.ToArray();
        }

        [Then]
        public void TheCustomCommandTypeShouldAppear()
        {
            _commandHandlerTypes.ShouldContain(typeof (SomeCommandTypeHandler));
        }

        public class SomeCommand : IBusCommand
        {
        }

        public class SomeCommandTypeHandler : IHandleCommand<SomeCommand>
        {
            public async Task Handle(SomeCommand busCommand)
            {
                throw new NotImplementedException();
            }
        }
    }
}