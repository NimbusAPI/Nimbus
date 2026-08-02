using System;
using System.Linq;
using Nimbus.Extensions.Pulse;
using Nimbus.Extensions.Pulse.Configuration;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.Tests.Unit.PulseTests
{
    [TestFixture]
    public class WhenConfiguringPulseSchedules
    {
        private static PulseScheduleEntry[] Build(Action<PulseScheduleCollection> configure)
        {
            var schedules = new PulseScheduleCollection();
            configure(schedules);
            return schedules.Build();
        }

        [Test]
        public void AScheduleWithoutANameShouldGetOneDerivedFromItsTypeAndCronExpression()
        {
            var entries = Build(p => p.Add("0 3 * * *", new SomePulseCommand()));

            entries.Single().Name.ShouldBe(PulseScheduleName.For(typeof(SomePulseCommand), "0 3 * * *"));
        }

        [Test]
        public void AnExplicitNameShouldBeUsedVerbatim()
        {
            var entries = Build(p => p.Add("0 3 * * *", new SomePulseCommand(), "nightly-rollup"));

            entries.Single().Name.ShouldBe("nightly-rollup");
        }

        [Test]
        public void AnExplicitNameShouldBeTrimmed()
        {
            var entries = Build(p => p.Add("0 3 * * *", new SomePulseCommand(), "  nightly-rollup  "));

            entries.Single().Name.ShouldBe("nightly-rollup");
        }

        [Test]
        public void ExplicitNamesShouldLetTheSameTypeRunOnTheSameCronExpressionTwice()
        {
            var entries = Build(p => p.Add("0 3 * * *", new SomePulseCommand(), "rollup-a")
                                      .Add("0 3 * * *", new SomePulseCommand(), "rollup-b"));

            entries.Select(e => e.Name).ShouldBe(new[] {"rollup-a", "rollup-b"});
        }

        [Test]
        public void CommandsAndEventsShouldBeDistinguished()
        {
            var entries = Build(p => p.Add("0 3 * * *", new SomePulseCommand())
                                      .Add("0 3 * * *", new SomePulseEvent()));

            entries.Single(e => e.Message is SomePulseCommand).IsCommand.ShouldBe(true);
            entries.Single(e => e.Message is SomePulseEvent).IsCommand.ShouldBe(false);
        }

        [Test]
        public void AMessageThatIsNeitherACommandNorAnEventShouldBeRejected()
        {
            var exception = Should.Throw<ArgumentException>(() => Build(p => p.Add("0 3 * * *", new SomethingThatIsNotAMessage())));

            exception.Message.ShouldContain("IBusCommand");
        }

        [Test]
        public void ANullMessageShouldBeRejected()
        {
            Should.Throw<ArgumentNullException>(() => Build(p => p.Add("0 3 * * *", null)));
        }

        [Test]
        public void AMissingCronExpressionShouldBeRejected()
        {
            Should.Throw<ArgumentException>(() => Build(p => p.Add("   ", new SomePulseCommand())));
        }

        [Test]
        public void ABlankExplicitNameShouldBeRejected()
        {
            var exception = Should.Throw<ArgumentException>(() => Build(p => p.Add("0 3 * * *", new SomePulseCommand(), "   ")));

            exception.Message.ShouldContain("cannot be blank");
        }

        [Test]
        public void AnExplicitNameContainingControlCharactersShouldBeRejected()
        {
            var exception = Should.Throw<ArgumentException>(() => Build(p => p.Add("0 3 * * *", new SomePulseCommand(), "nightly\trollup")));

            exception.Message.ShouldContain("control characters");
        }
    }
}
