using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nimbus.Configuration;
using Nimbus.Extensions;
using Nimbus.Tests.Common;
using Nimbus.Tests.Common.Extensions;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.Conventions
{
    [TestFixture]
    public class AllExtensionMethodsNotOnPublicInterfaces
    {
        [Test]
        [TestCaseSource(typeof (TestCases))]
        public void ShouldBeInternal(MethodInfo method)
        {
            method.IsAssembly.ShouldBe(true);
        }

        [Test]
        [TestCaseSource(typeof (TestCases))]
        public void ShouldBeOnAClassThatIsMarkedAsInternal(MethodInfo method)
        {
            method.DeclaringType.IsPublic.ShouldBe(false);
        }

        private class TestCases : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            {
                var assemblies = new[]
                                 {
                                     typeof (Bus).Assembly
                                 };

                var testCases = assemblies
                    .SelectMany(a => a.DefinedTypes)
                    .SelectMany(t => t.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static))
                    .Where(m => m.IsExtensionMethod())
                    .Where(m => !m.IsExtensionMethodFor<IBus>())
                    .Where(m => !m.IsExtensionMethodFor<INimbusConfiguration>())
                    .Where(m => !m.IsExtensionMethodFor<ITypeProvider>())
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