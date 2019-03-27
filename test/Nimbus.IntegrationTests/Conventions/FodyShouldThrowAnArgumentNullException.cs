using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Conventions
{
    [TestFixture]
    [Category("Convention")]
    public class FodyShouldThrowAnArgumentNullException
    {
        protected const int TimeoutSeconds = 15;

        [Test]
        [TestCaseSource(typeof (TestCases))]
        public async Task WhenPassingANullArgumentToAConstructor(Assembly assembly)
        {
            var fodyTestsType = GetFodyTestsType(assembly);
            fodyTestsType.ShouldNotBe(null);

            Exception ex = null;
            try
            {
                Activator.CreateInstance(fodyTestsType, new object[] {null});
            }
            catch (TargetInvocationException exc)
            {
                ex = exc.InnerException;
            }

            ex.ShouldBeOfType<ArgumentNullException>();
        }

        [Test]
        [TestCaseSource(typeof (TestCases))]
        public async Task WhenPassingANullArgumentToAPublicMethod(Assembly assembly)
        {
            var fodyTestsType = GetFodyTestsType(assembly);
            fodyTestsType.ShouldNotBe(null);

            var fodyTests = Activator.CreateInstance(fodyTestsType, "dummy");
            var method = fodyTestsType.GetMethod("DoFoo", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            Exception ex = null;
            try
            {
                method.Invoke(fodyTests, new object[] { null });
            }
            catch (TargetInvocationException exc)
            {
                ex = exc.InnerException;
            }

            ex.ShouldBeOfType<ArgumentNullException>();
        }

        [Test]
        [TestCaseSource(typeof (TestCases))]
        public async Task WhenPassingANullArgumentToAPrivateMethod(Assembly assembly)
        {
            var fodyTestsType = GetFodyTestsType(assembly);
            fodyTestsType.ShouldNotBe(null);

            var fodyTests = Activator.CreateInstance(fodyTestsType, "dummy");
            var method = fodyTestsType.GetMethod("DoBar", BindingFlags.Instance | BindingFlags.NonPublic);

            Exception ex = null;
            try
            {
                method.Invoke(fodyTests, new object[] {null});
            }
            catch (TargetInvocationException exc)
            {
                ex = exc.InnerException;
            }

            ex.ShouldBeOfType<ArgumentNullException>();
        }

        private static Type GetFodyTestsType(Assembly assembly)
        {
            return assembly
                .DefinedTypes
                .Where(t => t.Name == "FodyTests")
                .FirstOrDefault();
        }

        private class TestCases : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            {
                return AppDomainScanner.MyAssemblies
                                       .Where(a => !a.GetName().Name.Contains("Test"))
                                       .Where(a => !a.GetName().Name.Contains("MessageContracts"))
                                       .Where(a => !a.GetName().Name.Contains("InfrastructureContracts"))
                                       .Select(a => new TestCaseData(a).SetName(a.GetName().Name))
                                       .GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}