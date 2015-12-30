using System;
using System.Linq;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.MulticastRequestResponseTests
{
    [TestFixture]
    internal class WhenTakingThreeResponses : GivenAWrapperWithTwoResponses
    {
        private readonly TimeSpan _timeout = TimeSpan.FromMilliseconds(100);
        private readonly TimeSpan _acceptableTime = TimeSpan.FromMilliseconds(200);

        private string[] _result;

        protected override void When()
        {
            _result = Subject.ReturnResponsesOpportunistically(_timeout).Take(3).ToArray();
        }

        [Test]
        public void TheElapsedTimeShouldBeALittleGreaterThanTheTimeout()
        {
            ElapsedTime.ShouldBeGreaterThanOrEqualTo(_timeout);
            ElapsedTime.ShouldBeLessThan(_acceptableTime);
        }

        [Test]
        public void ThereShouldBeTwoResults()
        {
            _result.Count().ShouldBe(2);
        }
    }
}