using Nimbus.Infrastructure;
using Nimbus.MessageContracts.Exceptions;
using Nimbus.UnitTests.TestAssemblies.MessageContracts.Serialization;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.AssemblyScanningTests
{
    [TestFixture]
    public class WhenAUnserializableAMessageThatIsInAnAssemblyThatIsNotIncluded
    {
        [Test]
        [ExpectedException(typeof(BusException))]
        public void TheAssemblyScannerShouldGoBang()
        {
            var assemblyScanningTypeProvider = new AssemblyScanningTypeProvider(typeof(UnserializableCommandWhoseAssemblyShouldNotBeIncluded).Assembly);

            assemblyScanningTypeProvider.Verify();
        }

        [Test]
        public void TheExceptionShouldIncludeAnInnerException()
        {
            var assemblyScanningTypeProvider = new AssemblyScanningTypeProvider(typeof(UnserializableCommandWhoseAssemblyShouldNotBeIncluded).Assembly);

            var exception = Assert.Throws<BusException>(() => assemblyScanningTypeProvider.Verify());

            exception.InnerException.ShouldNotBe(null);
        }
    }
}