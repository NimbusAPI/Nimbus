using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nimbus.Configuration;
using Nimbus.Extensions;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.Conventions
{
    [TestFixture]
    public class AllBusBuilderConfigurationExtensionMethods
    {
        private readonly string _configurationExtensionsNamespace = typeof (BusBuilderConfigurationExtensions).Namespace;

        [Test]
        [TestCaseSource(typeof (TestCases))]
        public void ShouldBeInTheSameNamespace(MethodInfo method)
        {
            method.DeclaringType.Namespace.ShouldBe(_configurationExtensionsNamespace);
        }

        private class TestCases : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            {
                var assemblies = new[]
                {
                    typeof (BusBuilderConfigurationExtensions).Assembly,
                    typeof (AutofacBusBuilderConfigurationExtensions).Assembly,
                    typeof (WindsorBusBuilderConfigurationExtensions).Assembly
                };

                var testCases = assemblies
                    .SelectMany(a => a.GetExportedTypes())
                    .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
                    .Where(m => m.IsExtensionMethodFor<BusBuilderConfiguration>())
                    .Select(m => new TestCaseData(m)
                                .SetName("{0}.{1}".FormatWith(m.DeclaringType.FullName, m.Name))
                    );

                return testCases.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}