using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using NUnit.Framework;

namespace Nimbus.UnitTests.ParameterCheckingTests
{
    [TestFixture]
    public class FodyShouldThrowAnArgumentNullException
    {
        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void WhenPassingANullArgumentToAConstructor()
        {
            // ReSharper disable once ObjectCreationAsStatement
            new FodyTests(null);
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void WhenPassingANullArgumentToAPublicMethod()
        {
            var fodyTests = new FodyTests("dummy");
            fodyTests.DoFoo(null);
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void WhenPassingANullArgumentToAPrivateMethod()
        {
            var fodyTests = new FodyTests("dummy");
            var method = typeof (FodyTests).GetMethod("DoBar", BindingFlags.Instance | BindingFlags.NonPublic);
            try
            {
                method.Invoke(fodyTests, new object[] { null });
            }
            catch (TargetInvocationException exc)
            {
                ExceptionDispatchInfo.Capture(exc.InnerException).Throw();
            }
        }
    }
}