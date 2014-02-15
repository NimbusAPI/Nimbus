using System;
using System.Linq;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.MulticastRequestResponseTests
{
    [TestFixture]
    internal class WhenTakingATwoResponses : GivenAWrapperWithTwoResponses
    {
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(1);
        private readonly TimeSpan _acceptableTime = TimeSpan.FromMilliseconds(100);

        private string[] _result;

        public override void When()
        {
            _result = Subject.ReturnResponsesOpportunistically(_timeout).Take(2).ToArray();
        }

        [Test]
        public void TheElapsedTimeShouldBeMuchLessThanTheTimeout()
        {
            ElapsedTime.ShouldBeLessThan(_acceptableTime);
        }

        [Test]
        public void ThereShouldBeTwoResults()
        {
            _result.Count().ShouldBe(2);
        }
    }
}