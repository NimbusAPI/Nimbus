using Nimbus.InfrastructureContracts;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.Tests.Unit.Conventions
{
    [TestFixture]
    [Category("Convention")]
    public class CoreInfrastructureInterfaces
    {
        [Test]
        public void MustAdhereToConventions()
        {
            var coreInfrastructureInterfaces = new[]
                                               {
                                                   typeof(IBus)
                                               };

            foreach (var type in coreInfrastructureInterfaces)
            {
                type.Namespace.ShouldBe("Nimbus.InfrastructureContracts");
            }
        }
    }
}