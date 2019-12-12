using Nimbus.Infrastructure;
using Nimbus.Tests.Unit.TestAssemblies.Handlers;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.Tests.Unit.AssemblyScanningTests
{
    [TestFixture]
    public class WhenAHandlerReferencesAMessageThatIsInAnAssemblyThatIsNotIncluded
    {
        [Test]
        public void ValidationShouldFail()
        {
            var assemblyScanningTypeProvider = new AssemblyScanningTypeProvider(typeof (CommandWhoseAssemblyShouldNotBeIncludedHandler).Assembly);
            assemblyScanningTypeProvider.Validate().ShouldNotBeEmpty();
        }
    }
}