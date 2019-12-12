using System;
using System.Collections;
using System.Collections.Generic;
using Nimbus.Extensions;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.Tests.Unit.DateTimeOffsetTests
{
    public class WhenAddingValuesToADateTime
    {
        [Test]
        [TestCaseSource(typeof (TestCases))]
        public void TheAnswerShouldBeCorrect(DateTimeOffset a, TimeSpan b, DateTimeOffset c)
        {
            a.AddSafely(b).ShouldBe(c);
        }

        public class TestCases : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            {
                yield return new TestCaseData(DateTimeOffset.MaxValue, TimeSpan.MaxValue, DateTimeOffset.MaxValue);
                yield return new TestCaseData(DateTimeOffset.MaxValue, TimeSpan.FromSeconds(1), DateTimeOffset.MaxValue);
                yield return new TestCaseData(DateTimeOffset.MaxValue, TimeSpan.Zero, DateTimeOffset.MaxValue);
                yield return new TestCaseData(DateTimeOffset.MaxValue, TimeSpan.MinValue, DateTimeOffset.MinValue);

                yield return new TestCaseData(DateTimeOffset.MinValue, TimeSpan.MaxValue, DateTimeOffset.MaxValue);
                yield return new TestCaseData(DateTimeOffset.MinValue, TimeSpan.Zero, DateTimeOffset.MinValue);
                yield return new TestCaseData(DateTimeOffset.MinValue, TimeSpan.FromSeconds(-1), DateTimeOffset.MinValue);
                yield return new TestCaseData(DateTimeOffset.MinValue, TimeSpan.MinValue, DateTimeOffset.MinValue);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}