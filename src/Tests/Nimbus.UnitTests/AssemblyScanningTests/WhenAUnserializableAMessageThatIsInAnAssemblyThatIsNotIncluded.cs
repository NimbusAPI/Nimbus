using Nimbus.Infrastructure;
using Nimbus.MessageContracts.Exceptions;
using Nimbus.UnitTests.TestAssemblies.MessageContracts.Serialization;
using NUnit.Framework;

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
    }
}