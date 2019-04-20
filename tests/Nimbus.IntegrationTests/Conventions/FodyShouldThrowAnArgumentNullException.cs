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
    //[Timeout(TimeoutSeconds * 1000)]
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

            Should.Throw<TargetInvocationException>(() => 
            {
                Activator.CreateInstance(fodyTestsType, new object[] {null});
            });

        }

        [Test]
        [TestCaseSource(typeof (TestCases))]
        public async Task WhenPassingANullArgumentToAPublicMethod(Assembly assembly)
        {
            var fodyTestsType = GetFodyTestsType(assembly);
            fodyTestsType.ShouldNotBe(null);

            var fodyTests = Activator.CreateInstance(fodyTestsType, "dummy");
            var method = fodyTestsType.GetMethod("DoFoo", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            Should.Throw<ArgumentNullException>(() => 
            {
                method.Invoke(fodyTests, new object[] { null });
            });

            // Exception ex = null;
            // try
            // {
            //     method.Invoke(fodyTests, new object[] { null });
            // }
            // catch (TargetInvocationException exc)
            // {
            //     ex = exc.InnerException;
            // }

            // ex.ShouldBeTypeOf<ArgumentNullException>();
        }

        [Test]
        [TestCaseSource(typeof (TestCases))]
        public async Task WhenPassingANullArgumentToAPrivateMethod(Assembly assembly)
        {
            var fodyTestsType = GetFodyTestsType(assembly);
            fodyTestsType.ShouldNotBe(null);

            var fodyTests = Activator.CreateInstance(fodyTestsType, "dummy");
            var method = fodyTestsType.GetMethod("DoBar", BindingFlags.Instance | BindingFlags.NonPublic);

            Should.Throw<ArgumentNullException>(() => 
            {
                method.Invoke(fodyTests, new object[] { null });
            });

            // Exception ex = null;
            // try
            // {
            //     method.Invoke(fodyTests, new object[] {null});
            // }
            // catch (TargetInvocationException exc)
            // {
            //     ex = exc.InnerException;
            // }

            // ex.ShouldBeTypeOf<ArgumentNullException>();
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