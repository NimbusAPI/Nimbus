using System;
using System.Reflection;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.ParameterCheckingTests
{
    [TestFixture]
    public class FodyShouldThrowAnArgumentNullException
    {
        [Test]
        public void WhenPassingANullArgumentToAConstructor()
        {
            // ReSharper disable once ObjectCreationAsStatement
            Should.Throw<ArgumentNullException>(() => new FodyTests(null));
        }

        [Test]
        public void WhenPassingANullArgumentToAPublicMethod()
        {
            var fodyTests = new FodyTests("dummy");
            Should.Throw<ArgumentNullException>(() => fodyTests.DoFoo(null));
        }

        [Test]
        public void WhenPassingANullArgumentToAPrivateMethod()
        {
            var fodyTests = new FodyTests("dummy");
            var method = typeof (FodyTests).GetMethod("DoBar", BindingFlags.Instance | BindingFlags.NonPublic);
            Exception ex = null;

            try
            {
                method.Invoke(fodyTests, new object[] {null});
            }
            catch (TargetInvocationException exc)
            {
                ex = exc.InnerException;
            }

            ex.ShouldBeTypeOf<ArgumentNullException>();
        }
    }
}