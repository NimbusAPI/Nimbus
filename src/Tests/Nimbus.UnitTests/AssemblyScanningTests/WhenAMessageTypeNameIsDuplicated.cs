using System.Reflection;
using Nimbus.Infrastructure;
using Nimbus.UnitTests.TestAssemblies.MessageContracts;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.AssemblyScanningTests
{
    [TestFixture]
    public class WhenAMessageTypeNameIsDuplicated
    {
        [Test]
        public void ValidationShouldFail()
        {
            var assemblyScanningTypeProvider = new AssemblyScanningTypeProvider(Assembly.GetAssembly(typeof (DuplicateMessageType)));
            var typeProviderValidator = new TypeProviderValidator(assemblyScanningTypeProvider, new PathFactory());
            typeProviderValidator.Validate().ShouldNotBeEmpty();
        }
    }
}