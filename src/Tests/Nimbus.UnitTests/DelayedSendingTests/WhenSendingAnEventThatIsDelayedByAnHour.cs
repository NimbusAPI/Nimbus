using System;
using Nimbus.Infrastructure;
using Nimbus.UnitTests.DelayedSendingTests.MessageContracts;
using NSubstitute;
using NUnit.Framework;

namespace Nimbus.UnitTests.DelayedSendingTests
{
    [TestFixture]
    public class WhenSendingAnEventThatIsDelayedByAnHour : SpecificationFor<IBus>
    {
        private FooEvent _fooEvent;
        private DateTimeOffset _expectedDeliveryTime;

        protected override IBus Given()
        {
            return Substitute.For<IBus>();
        }

        protected override void When()
        {
            var lyingClock = Substitute.For<IClock>();
            var now = new DateTimeOffset(2014, 02, 09, 18, 04, 58, TimeSpan.FromHours(10));
            lyingClock.UtcNow.ReturnsForAnyArgs(now);

            DelayedSendingExtensions.SetClockStrategy(lyingClock);

            _fooEvent = new FooEvent();
            var delay = TimeSpan.FromDays(2);
            _expectedDeliveryTime = now.Add(delay);

            Subject.PublishAfter(_fooEvent, TimeSpan.FromDays(2));
        }

        [Test]
        public void TheCalculatedTimeShouldBeCorrect()
        {
            Subject.Received().PublishAt(_fooEvent, _expectedDeliveryTime);
        }
    }
}