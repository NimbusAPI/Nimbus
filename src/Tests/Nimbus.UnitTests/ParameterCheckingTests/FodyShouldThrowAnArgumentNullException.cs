using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
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
            try
            {
                Should.Throw<ArgumentNullException>(() => method.Invoke(fodyTests, new object[] {null}));
            }
            catch (TargetInvocationException exc)
            {
                ExceptionDispatchInfo.Capture(exc.InnerException).Throw();
            }
        }
    }
}