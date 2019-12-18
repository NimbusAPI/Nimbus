using System;
using System.Linq;
using Conventional;
using Nimbus.Infrastructure;
using NUnit.Framework;

namespace Nimbus.Tests.Unit.Conventions
{
    [TestFixture]
    [Category("Convention")]
    public class AllInterfacesInTheInfrastructureNamespace
    {
        [Test]
        public void MustAdhereToConventions()
        {
            var referenceType = typeof(IMessagePump);

            referenceType.Assembly.GetTypes()
                         .Where(t => t.Namespace != null)
                         .Where(t => t.Namespace == referenceType.Namespace || t.Namespace.StartsWith(referenceType.Namespace + "."))
                         .Where(t => t.IsInterface)
                         .MustConformTo(new MustBeInternalConventionSpecification())
                         .WithFailureAssertion(message => throw new Exception(message));
        }
    }
}