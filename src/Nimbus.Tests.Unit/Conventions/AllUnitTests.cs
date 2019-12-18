using System;
using System.Linq;
using Conventional;
using NUnit.Framework;

namespace Nimbus.Tests.Unit.Conventions
{
    [TestFixture]
    [Category("Convention")]
    public class AllUnitTests
    {
        [Test]
        public void MustAdhereToConventions()
        {
            GetType().Assembly
                     .DefinedTypes
                     .Where(t => t.IsTestClass())
                     .MustConformTo(Convention.VoidMethodsMustNotBeAsync)
                     .WithFailureAssertion(message => throw new Exception(message));
        }
    }
}