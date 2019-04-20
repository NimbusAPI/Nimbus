using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Nimbus.Extensions;
using Nimbus.IntegrationTests.Extensions;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Conventions
{
    [TestFixture]
    //[Timeout(TimeoutSeconds*1000)]
    [Category("Convention")]
    public class AllTestsForBus
    {
        protected const int TimeoutSeconds = 15;

        [Test]
        [TestCaseSource(typeof (TestCases))]
        public async Task ShouldHaveASingleTestNamedRun(Type fixtureType)
        {
            var testMethods = fixtureType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                         .Where(m => m.HasAttribute<TestAttribute>())
                                         .ToArray();

            testMethods.Single().Name.ShouldBe("Run");
        }

        [Test]
        [TestCaseSource(typeof (TestCases))]
        public async Task ShouldHaveAtLeastOneMethodWithAThenAttribute(Type fixtureType)
        {
            var thenMethods = fixtureType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                         .Where(m => m.HasAttribute<ThenAttribute>())
                                         .ToArray();

            thenMethods.Length.ShouldBeGreaterThan(0);
        }

        private class TestCases : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            {
                return GetType().Assembly
                                .DefinedTypes
                                .Where(t => t.IsAssignableTo<TestForBus>())
                                .Where(t => t.IsInstantiable())
                                .Select(t => new TestCaseData(t).SetName(t.FullName))
                                .OrderBy(tc => tc.TestName)
                                .GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}