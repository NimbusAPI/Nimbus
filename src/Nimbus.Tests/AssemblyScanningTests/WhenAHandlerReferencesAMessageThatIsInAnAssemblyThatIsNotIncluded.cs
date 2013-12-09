using Nimbus.Infrastructure;
using Nimbus.MessageContracts.Exceptions;
using Nimbus.Tests.TestAssemblies.Handlers;
using NUnit.Framework;

namespace Nimbus.Tests.AssemblyScanningTests
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