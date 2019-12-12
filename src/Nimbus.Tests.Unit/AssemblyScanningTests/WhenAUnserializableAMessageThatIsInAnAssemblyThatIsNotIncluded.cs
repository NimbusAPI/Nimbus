using System.Linq;
using Nimbus.Infrastructure;
using Nimbus.Tests.Unit.TestAssemblies.MessageContracts.Serialization;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.Tests.Unit.AssemblyScanningTests
{
    [TestFixture]
    public class WhenAUnserializableAMessageThatIsInAnAssemblyThatIsNotIncluded
    {
        [Test]
        public void ValidationShouldFail()
        {
            var assemblyScanningTypeProvider = new AssemblyScanningTypeProvider(typeof (UnserializableCommandWhoseAssemblyShouldNotBeIncluded).Assembly);
            assemblyScanningTypeProvider.Validate().ShouldNotBeEmpty();
        }

        [Test]
        public void TheMessageShouldMentionTheOffendingTypeByName()
        {
            var assemblyScanningTypeProvider = new AssemblyScanningTypeProvider(typeof (UnserializableCommandWhoseAssemblyShouldNotBeIncluded).Assembly);

            var validationErrors = assemblyScanningTypeProvider.Validate().ToArray();

            validationErrors.ShouldContain(e => e.Contains(typeof (UnserializableCommandWhoseAssemblyShouldNotBeIncluded).FullName));
        }
    }
}