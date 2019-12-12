using System;
using System.Linq;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.MulticastRequestResponseTests
{
    [TestFixture]
    internal class WhenTakingASingleResponse : GivenAWrapperWithTwoResponses
    {
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(1.5);

        private string[] _result;

        protected override void When()
        {
            _result = Subject.ReturnResponsesOpportunistically(_timeout).Take(1).ToArray();
        }

        [Test]
        public void TheElapsedTimeShouldBeLessThanTheTimeout()
        {
            ElapsedTime.ShouldBeLessThan(_timeout);
        }

        [Test]
        public void ThereShouldBeASingleResult()
        {
            _result.Count().ShouldBe(1);
        }
    }
}