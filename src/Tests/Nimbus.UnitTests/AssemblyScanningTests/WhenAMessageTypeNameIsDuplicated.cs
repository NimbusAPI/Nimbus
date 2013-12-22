using System.Reflection;
using Nimbus.Infrastructure;
using Nimbus.MessageContracts.Exceptions;
using Nimbus.UnitTests.TestAssemblies.MessageContracts;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.AssemblyScanningTests
{
    [TestFixture]
    public class WhenAMessageTypeNameIsDuplicated
    {
        [Test]
        public void TheTypeScannerShouldThrowOnVerify()
        {
            var scanner = new AssemblyScanningTypeProvider(Assembly.GetAssembly(typeof (DuplicateMessageType)));

            Should.Throw<BusException>(scanner.Verify);
        }
    }
}