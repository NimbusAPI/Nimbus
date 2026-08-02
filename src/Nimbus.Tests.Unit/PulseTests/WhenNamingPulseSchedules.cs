using Nimbus.Extensions.Pulse;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.Tests.Unit.PulseTests
{
    [TestFixture]
    public class WhenNamingPulseSchedules
    {
        [Test]
        public void TheNameShouldIncludeTheMessageTypeAndTheCronExpression()
        {
            var name = PulseScheduleName.For(typeof(SomePulseCommand), "0 3 * * *");

            name.ShouldBe("Nimbus.Tests.Unit.PulseTests.SomePulseCommand:0_3_*_*_*");
        }

        [Test]
        public void ExtraWhitespaceShouldNotChangeTheName()
        {
            var name = PulseScheduleName.For(typeof(SomePulseCommand), "0   3 *  * *");

            name.ShouldBe(PulseScheduleName.For(typeof(SomePulseCommand), "0 3 * * *"));
        }

        [Test]
        public void LeadingAndTrailingWhitespaceShouldNotChangeTheName()
        {
            var name = PulseScheduleName.For(typeof(SomePulseCommand), "  0 3 * * *  ");

            name.ShouldBe(PulseScheduleName.For(typeof(SomePulseCommand), "0 3 * * *"));
        }

        [Test]
        public void ADifferentCronExpressionShouldProduceADifferentName()
        {
            var threeAm = PulseScheduleName.For(typeof(SomePulseCommand), "0 3 * * *");
            var fourAm = PulseScheduleName.For(typeof(SomePulseCommand), "0 4 * * *");

            threeAm.ShouldNotBe(fourAm);
        }

        [Test]
        public void ADifferentMessageTypeShouldProduceADifferentName()
        {
            var command = PulseScheduleName.For(typeof(SomePulseCommand), "0 3 * * *");
            var @event = PulseScheduleName.For(typeof(SomePulseEvent), "0 3 * * *");

            command.ShouldNotBe(@event);
        }

        [Test]
        public void TypesWithTheSameShortNameInDifferentNamespacesShouldNotCollide()
        {
            var outer = PulseScheduleName.For(typeof(SomePulseCommand), "0 3 * * *");
            var nested = PulseScheduleName.For(typeof(Nested.SomePulseCommand), "0 3 * * *");

            outer.ShouldNotBe(nested);
        }

        public static class Nested
        {
            public class SomePulseCommand : MessageContracts.IBusCommand
            {
            }
        }
    }
}
