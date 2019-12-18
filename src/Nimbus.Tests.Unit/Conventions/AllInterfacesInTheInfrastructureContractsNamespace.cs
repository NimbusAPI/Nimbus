using System;
using System.Linq;
using Conventional;
using Nimbus.InfrastructureContracts.DependencyResolution;
using NUnit.Framework;

namespace Nimbus.Tests.Unit.Conventions
{
    [TestFixture]
    [Category("Convention")]
    public class AllInterfacesInTheInfrastructureContractsNamespace
    {
        [Test]
        public void MustAdhereToConventions()
        {
            var referenceType = typeof(IDependencyResolver);

            referenceType.Assembly.GetTypes()
                         .Where(t => t.Namespace != null)
                         .Where(t => t.Namespace == referenceType.Namespace || t.Namespace.StartsWith(referenceType.Namespace + "."))
                         .Where(t => t.IsInterface)
                         .MustConformTo(new MustBePublicConventionSpecification())
                         .WithFailureAssertion(message => throw new Exception(message));
        }
    }
}