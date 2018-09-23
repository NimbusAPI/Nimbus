using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nimbus.Configuration;
using Nimbus.Configuration.LargeMessages;
using Nimbus.Extensions;
using Nimbus.Tests.Common;
using Nimbus.Tests.Common.Extensions;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.Conventions
{
    [TestFixture]
    [Category("Convention")]
    public class AllBusBuilderConfigurationExtensionMethods
    {
        private readonly string _configurationExtensionsNamespace = typeof (BusBuilderConfigurationExtensions).Namespace;

        [Test]
        [TestCaseSource(typeof (TestCases))]
        public void ShouldBeInTheSameNamespace(MethodInfo method)
        {
            method.DeclaringType.Namespace.ShouldBe(_configurationExtensionsNamespace);
        }

        [Test]
        [TestCaseSource(typeof (TestCases))]
        public void ShouldBePublic(MethodInfo method)
        {
            method.IsPublic.ShouldBe(true);
        }

        private class TestCases : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            {
                var assemblies = new[]
                                 {
                                     typeof (BusBuilderConfigurationExtensions).Assembly,
                                     typeof (AutofacBusBuilderConfigurationExtensions).Assembly,
                                 };

                var testCases = assemblies
                    .SelectMany(a => a.DefinedTypes)
                    .SelectMany(t => t.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static))
                    .Where(m => m.IsExtensionMethodFor<INimbusConfiguration>())
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