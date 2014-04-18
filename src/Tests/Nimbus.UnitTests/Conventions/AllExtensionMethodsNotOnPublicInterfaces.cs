using System;
using NUnit.Framework;

namespace Nimbus.UnitTests.Conventions
{
    [TestFixture]
    public class AllExtensionMethodsNotOnPublicInterfaces
    {
        [Test]
        public void ShouldBeInternal()
        {
            throw new NotImplementedException();
        }
    }
}