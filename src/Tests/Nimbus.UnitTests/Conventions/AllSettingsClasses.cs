using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.Conventions
{
    [TestFixture]
    [Category("Convention")]
    public class AllSettingsClasses
    {
        private readonly string _referenceNamespace = typeof (Setting<>).Namespace;

        [Test]
        [TestCaseSource(typeof (TestCases))]
        public void ShouldBeInASettingsNamespaceUnderConfiguration(Type settingType)
        {
            settingType.Namespace.ShouldContain(".Configuration.");
            settingType.Namespace.ShouldEndWith(".Settings");
        }

        [Test]
        [TestCaseSource(typeof (TestCases))]
        public void ShouldBePublic(Type settingType)
        {
            settingType.IsPublic.ShouldBe(true);
        }

        internal class TestCases : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            {
                return typeof (Setting<>).Assembly
                                         .GetTypes()
                                         .Where(t => t.IsClosedTypeOf(typeof (Setting<>)))
                                         .Select(t => new TestCaseData(t)
                                                     .SetName(t.FullName))
                                         .GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}