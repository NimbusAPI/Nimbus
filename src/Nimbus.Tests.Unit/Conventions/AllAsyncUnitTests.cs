using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Nimbus.Extensions;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.Tests.Unit.Conventions
{
    [TestFixture]
    [Category("Convention")]
    public class AllAsyncUnitTests
    {
        [Test]
        [TestCaseSource(typeof (TestCases))]
        public void ShouldReturnTask(MethodInfo testMethod)
        {
            typeof (Task).IsAssignableFrom(testMethod.ReturnType).ShouldBe(true);
        }

        [Test]
        [TestCaseSource(typeof (TestCases))]
        public void ShouldBeAsync(MethodInfo testMethod)
        {
            testMethod.HasAttribute<AsyncStateMachineAttribute>().ShouldBe(true);
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
                                .Where(m => m.HasAttribute<AsyncStateMachineAttribute>())
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