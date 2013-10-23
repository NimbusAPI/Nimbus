using System;
using System.Linq;
using Nimbus.Configuration;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts;
using Shouldly;

namespace Nimbus.Tests.ConfigurationTests
{
    public class WhenScanningAssembliesForHandledCommandTypes : SpecificationFor<AssemblyScanningTypeProvider>
    {
        private Type[] _commandHandlerTypes;

        public override AssemblyScanningTypeProvider Given()
        {
            return new AssemblyScanningTypeProvider(typeof (WhenScanningAssembliesForHandledCommandTypes).Assembly);
        }

        public override void When()
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
            public void Handle(SomeCommand busCommand)
            {
                throw new NotImplementedException();
            }
        }
    }
}