using System.Reflection;
using Nimbus.Infrastructure;
using Nimbus.Tests.Unit.TestAssemblies.MessageContracts;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.Tests.Unit.AssemblyScanningTests
{
    [TestFixture]
    public class WhenAMessageTypeNameIsDuplicated
    {
        [Test]
        public void ValidationShouldFail()
        {
            var assemblyScanningTypeProvider = new AssemblyScanningTypeProvider(Assembly.GetAssembly(typeof (DuplicateMessageType)));
            assemblyScanningTypeProvider.Validate().ShouldNotBeEmpty();
        }
    }
}