using System;
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
    public class AllComponentsThatCreateOtherComponents
    {
        [Test]
        [TestCaseSource(typeof (TestCases))]
        public void MustHaveAGarbageMan(Type componentType)
        {
            componentType.GetField("_garbageMan", BindingFlags.Instance | BindingFlags.NonPublic).ShouldNotBe(null);
        }

        internal class TestCases : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            {
                return typeof (Bus).Assembly
                                   .GetTypes()
                                   .Where(t => typeof (ICreateComponents).IsAssignableFrom(t))
                                   .Where(t => t.IsInstantiable())
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