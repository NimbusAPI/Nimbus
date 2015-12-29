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
        ///     ConcurrentDictionary doesn't prevent multiple creation of items for non-existing keys. Use ThreadSafeDictionary
        ///     instead.
        /// </summary>
        [Test]
        [TestCaseSource(typeof (TestCases))]
        public async Task ShouldNeverUseConcurrentDictionary(Type type)
        {
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            fields.Where(f => f.FieldType.IsClosedTypeOf(typeof (ConcurrentDictionary<,>))).ShouldBeEmpty();
        }

        /// <summary>
        ///     Lazy doesn't prevent multiple creation of items when the Lazy object is uninitialized. Use ThreadSafeLazy instead.
        /// </summary>
        [Test]
        [TestCaseSource(typeof (TestCases))]
        public async Task ShouldNeverUseLazy(Type type)
        {
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            fields.Where(f => f.FieldType.IsClosedTypeOf(typeof (Lazy<>))).ShouldBeEmpty();
        }

        /// <summary>
        ///     BlockingCollection blocks the thread, which is a Bad Thing. Use AsyncBlockingCollection instead.
        /// </summary>
        [Test]
        [TestCaseSource(typeof (TestCases))]
        public async Task ShouldNeverUseBlockingCollection(Type type)
        {
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            fields.Where(f => f.FieldType.IsClosedTypeOf(typeof (BlockingCollection<>))).ShouldBeEmpty();
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