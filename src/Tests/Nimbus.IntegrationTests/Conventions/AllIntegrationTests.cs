using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Nimbus.Extensions;
using Nimbus.IntegrationTests.Extensions;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Conventions
{
    [TestFixture]
    [Timeout(TimeoutSeconds*1000)]
    [Category("Convention")]
    public class AllIntegrationTests
    {
        protected const int TimeoutSeconds = 15;

        [Test]
        [TestCaseSource(typeof (TestCases))]
        public async Task ShouldReturnTask(MethodInfo testMethod)
        {
            typeof (Task).IsAssignableFrom(testMethod.ReturnType).ShouldBe(true);
        }

        [Test]
        [TestCaseSource(typeof (TestCases))]
        public async Task ShouldBeAsync(MethodInfo testMethod)
        {
            testMethod.HasAttribute<AsyncStateMachineAttribute>().ShouldBe(true);
        }

        [Test]
        [TestCaseSource(typeof (TestCases))]
        public async Task ShouldHaveATimeout(MethodInfo testMethod)
        {
            if (testMethod.HasAttribute<TimeoutAttribute>()) return;

            var fixtureTypeHeirarchy = new[] {testMethod.DeclaringType}.DepthFirst(t => t.BaseType != null ? new[] {t.BaseType} : new Type[0]);
            if (fixtureTypeHeirarchy.Any(t => t.HasAttribute<TimeoutAttribute>())) return;

            Assert.Fail();
        }

        private class TestCases : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            {
                return GetType().Assembly
                                .GetExportedTypes()
                                .Where(t => t.GetCustomAttributes<TestFixtureAttribute>().Any())
                                .SelectMany(t => t.GetMethods())
                                .Where(m => m.GetCustomAttributes<TestAttribute>().Any())
                                .Select(m => new TestCaseData(m)
                                            .SetName("{0}.{1}".FormatWith(m.DeclaringType.FullName, m.Name))
                    )
                                .GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}