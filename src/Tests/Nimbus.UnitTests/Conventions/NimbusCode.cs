using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Nimbus.Extensions;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.Conventions
{
    [TestFixture]
    public class NimbusCode
    {
        /// <summary>
        /// ConcurrentDictionary doesn't prevent multiple creation of items for non-existing keys. Use ThreadSafeDictionary instead.
        /// </summary>
        [Test]
        [TestCaseSource(typeof (TestCases))]
        public async Task ShouldNeverUseConcurrentDictionary(Type type)
        {
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            fields.Where(f => f.FieldType.IsClosedTypeOf(typeof (ConcurrentDictionary<,>))).ShouldBeEmpty();
        }

        internal class TestCases : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            {
                return typeof (Bus).Assembly
                                   .GetTypes()
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