using Nimbus.Infrastructure;
using Nimbus.MessageContracts.Exceptions;
using Nimbus.UnitTests.TestAssemblies.Handlers;
using NUnit.Framework;

namespace Nimbus.UnitTests.AssemblyScanningTests
{
    [TestFixture]
    public class WhenAHandlerReferencesAMessageThatIsInAnAssemblyThatIsNotIncluded
    {
        [Test]
        [ExpectedException(typeof (BusException))]
        public void TheAssemblyScannerShouldGoBang()
        {
            var assemblyScanningTypeProvider = new AssemblyScanningTypeProvider(typeof (CommandWhoseAssemblyShouldNotBeIncludedHandler).Assembly);

            assemblyScanningTypeProvider.Verify();
        }
    }
}