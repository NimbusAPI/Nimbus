using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.Conventions
{
    [TestFixture]
    public class TheBusInterface
    {
        [Test]
        public void MustBeInTheCoreNimbusNamespace()
        {
            typeof(IBus).Namespace.ShouldBe("Nimbus");
        }
    }
}