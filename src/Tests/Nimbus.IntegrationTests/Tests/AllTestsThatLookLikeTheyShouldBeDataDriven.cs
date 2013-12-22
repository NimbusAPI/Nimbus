using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nimbus.Extensions;
using Nimbus.IntegrationTests.InfrastructureContracts;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests
{
    [TestFixture]
    public class AllTestsThatLookLikeTheyShouldBeDataDriven
    {
        [Test]
        [TestCaseSource(typeof (TestCases))]
        public void ShouldHaveTestCaseSourceAttributes(MethodInfo testMethod)
        {
            testMethod.GetCustomAttribute<TestCaseSourceAttribute>().ShouldNotBe(null);
        }

        [Test]
        [TestCaseSource(typeof (TestCases))]
        public void ShouldTakeATestHarnessBusFactoryAsAParameter(MethodInfo testMethod)
        {
            testMethod.GetParameters().ShouldContain(p => p.ParameterType == typeof (ITestHarnessBusFactory));
        }

        public class TestCases : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            {
                return GetType().Assembly
                                .GetExportedTypes()
                                .Where(t => t.GetCustomAttributes<TestFixtureAttribute>().Any())
                                .Where(t => typeof (TestForAllBuses).IsAssignableFrom(t))
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