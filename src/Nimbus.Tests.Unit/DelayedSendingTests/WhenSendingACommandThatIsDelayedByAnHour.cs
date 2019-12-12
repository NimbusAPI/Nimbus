using System;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;
using Nimbus.Tests.Unit.DelayedSendingTests.MessageContracts;
using NSubstitute;
using NUnit.Framework;

namespace Nimbus.Tests.Unit.DelayedSendingTests
{
    [TestFixture]
    public class WhenSendingACommandThatIsDelayedByAnHour : SpecificationFor<IBus>
    {
        private FooCommand _fooCommand;
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

            _fooCommand = new FooCommand();
            var delay = TimeSpan.FromDays(2);
            _expectedDeliveryTime = now.Add(delay);

            Subject.SendAfter(_fooCommand, TimeSpan.FromDays(2));
        }

        [Test]
        public void TheCalculatedTimeShouldBeCorrect()
        {
            Subject.Received().SendAt(_fooCommand, _expectedDeliveryTime);
        }
    }
}