using System;
using System.Linq;
using Conventional;
using Nimbus.InfrastructureContracts.Handlers;
using NUnit.Framework;

namespace Nimbus.Tests.Unit.Conventions
{
    [TestFixture]
    [Category("Convention")]
    public class AllGenericTypeConstraintsInTheInfrastructureNamespace
    {
        [Test]
        public void MustAdhereToConventions()
        {
            var referenceType = typeof(IHandleCommand<>);

            var interfaceTypes = referenceType.Assembly.GetTypes()
                                              .Where(t => t.Namespace.StartsWith(referenceType.Namespace))
                                              .Where(t => t.IsInterface);

            interfaceTypes.MustConformTo(new GenericTypeConstraintsMustBeInterfacesConventionSpecification())
                          .WithFailureAssertion(message => throw new Exception(message));
        }
    }
}