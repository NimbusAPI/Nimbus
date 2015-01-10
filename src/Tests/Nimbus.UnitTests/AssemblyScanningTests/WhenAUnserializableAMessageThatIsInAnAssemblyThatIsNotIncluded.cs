using System.Linq;
using Nimbus.Infrastructure;
using Nimbus.UnitTests.TestAssemblies.MessageContracts.Serialization;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.AssemblyScanningTests
{
    [TestFixture]
    public class WhenAUnserializableAMessageThatIsInAnAssemblyThatIsNotIncluded
    {
        [Test]
        public void ValidationShouldFail()
        {
            var assemblyScanningTypeProvider = new AssemblyScanningTypeProvider(typeof (UnserializableCommandWhoseAssemblyShouldNotBeIncluded).Assembly);
            var typeProviderValidator = new TypeProviderValidator(new PathGenerator(),assemblyScanningTypeProvider);
            typeProviderValidator.Validate().ShouldNotBeEmpty();
        }

        [Test]
        public void TheMessageShouldMentionTheOffendingTypeByName()
        {
            var assemblyScanningTypeProvider = new AssemblyScanningTypeProvider(typeof (UnserializableCommandWhoseAssemblyShouldNotBeIncluded).Assembly);
            var typeProviderValidator = new TypeProviderValidator(new PathGenerator(), assemblyScanningTypeProvider);
            var validationErrors = typeProviderValidator.Validate().ToArray();

            validationErrors.ShouldContain(e => e.Contains(typeof (UnserializableCommandWhoseAssemblyShouldNotBeIncluded).FullName));
        }
    }
}