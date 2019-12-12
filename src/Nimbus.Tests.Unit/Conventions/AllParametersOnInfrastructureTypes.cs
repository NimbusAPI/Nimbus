using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Nimbus.Configuration.Settings;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.Tests.Unit.Conventions
{
    [TestFixture]
    [Category("Convention")]
    public class AllParametersOnInfrastructureTypes
    {
        [Test]
        [TestCaseSource(typeof (TestCases))]
        public void ShouldBeInADeterministicOrder(ConstructorInfo constructor)
        {
            var constructorParameters = constructor.GetParameters();

            var expectedOrder = SortParameters(constructorParameters);
            var actualOrder = constructorParameters;

            actualOrder.ShouldBe(expectedOrder);
        }

        private ParameterInfo[] SortParameters(IEnumerable<ParameterInfo> constructorParameters)
        {
            return constructorParameters
                .OrderByDescending(p => p.ParameterType.IsClosedTypeOf(typeof (Setting<>)))
                .ThenBy(p => !p.ParameterType.IsInterface)
                .ThenBy(p => p.ParameterType.Name)
                .ThenBy(p => p.Name)
                .ToArray();
        }

        public class TestCases : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            {
                return typeof (Bus).Assembly
                                   .DefinedTypes
                                   .SelectMany(t => t.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                                   .Where(c => c.GetParameters().Any())
                                   .Where(IsInfrastructureConstructor)
                                   .Where(c => !IsIgnored(c.DeclaringType))
                                   .Select(c => new TestCaseData(c)
                                               .SetName(GenerateTestName(c)))
                                   .OrderBy(t => t.TestName)
                                   .GetEnumerator();
            }

            private bool IsIgnored(Type declaringType)
            {
                var ignoredTypes = new[] {typeof (Bus)};
                return ignoredTypes.Any(t => t.IsAssignableFrom(declaringType));
            }

            private bool IsInfrastructureConstructor(ConstructorInfo constructorInfo)
            {
                var parameters = constructorInfo.GetParameters();
                return parameters.All(p => (p.ParameterType.Namespace ?? string.Empty).StartsWith("Nimbus"));
            }

            private static string GenerateTestName(ConstructorInfo constructorInfo)
            {
                var descriptiveParameters = constructorInfo
                    .GetParameters()
                    .Select(p => p.ParameterType.Name + " " + p.Name)
                    .Join(", ");

                return constructorInfo.DeclaringType.FullName + "(" + descriptiveParameters + ")";
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}