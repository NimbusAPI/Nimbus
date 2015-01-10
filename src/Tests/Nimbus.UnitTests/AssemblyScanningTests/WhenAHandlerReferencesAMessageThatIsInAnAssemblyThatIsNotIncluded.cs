using Nimbus.Infrastructure;
using Nimbus.UnitTests.TestAssemblies.Handlers;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.AssemblyScanningTests
{
    [TestFixture]
    public class WhenAHandlerReferencesAMessageThatIsInAnAssemblyThatIsNotIncluded
    {
        [Test]
        public void ValidationShouldFail()
        {
            var assemblyScanningTypeProvider = new AssemblyScanningTypeProvider(typeof (CommandWhoseAssemblyShouldNotBeIncludedHandler).Assembly);
            assemblyScanningTypeProvider.ValidateSelf().ShouldNotBeEmpty();
        }
    }
}