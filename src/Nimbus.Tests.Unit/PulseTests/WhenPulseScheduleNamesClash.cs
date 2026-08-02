using System;
using Nimbus.Extensions.Pulse.Configuration;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.Tests.Unit.PulseTests
{
    /// <summary>
    ///     A clash means two schedules would race for the same coordinator claim and only one of them
    ///     would ever fire, so every flavour of it has to be caught at configuration time.
    /// </summary>
    [TestFixture]
    public class WhenPulseScheduleNamesClash
    {
        private static Exception BuildAndCatch(Action<PulseScheduleCollection> configure)
        {
            return Should.Throw<ArgumentException>(() =>
                                                   {
                                                       var schedules = new PulseScheduleCollection();
                                                       configure(schedules);
                                                       schedules.Build();
                                                   });
        }

        [Test]
        public void TwoDerivedNamesThatMatchShouldBeRejected()
        {
            var exception = BuildAndCatch(p => p.Add("0 3 * * *", new SomePulseCommand())
                                                .Add("0 3 * * *", new SomePulseCommand()));

            exception.Message.ShouldContain("Give at least one of them an explicit name");
        }

        [Test]
        public void TwoDerivedNamesThatMatchOnlyAfterWhitespaceNormalisationShouldBeRejected()
        {
            BuildAndCatch(p => p.Add("0 3 * * *", new SomePulseCommand())
                                .Add("0    3 * * *", new SomePulseCommand()));
        }

        [Test]
        public void TwoExplicitNamesThatMatchShouldBeRejected()
        {
            var exception = BuildAndCatch(p => p.Add("0 3 * * *", new SomePulseCommand(), "rollup")
                                                .Add("0 4 * * *", new SomePulseEvent(), "rollup"));

            exception.Message.ShouldContain("Explicit schedule names must be unique");
        }

        [Test]
        public void TwoExplicitNamesThatMatchOnlyAfterTrimmingShouldBeRejected()
        {
            BuildAndCatch(p => p.Add("0 3 * * *", new SomePulseCommand(), "rollup")
                                .Add("0 4 * * *", new SomePulseEvent(), "  rollup  "));
        }

        /// <summary>
        ///     Redis keys are case-sensitive, but a SQL Server coordinator running a case-insensitive
        ///     collation would fold these together. Rejecting them keeps a schedule set portable across
        ///     coordinator backends.
        /// </summary>
        [Test]
        public void TwoExplicitNamesDifferingOnlyByCaseShouldBeRejected()
        {
            BuildAndCatch(p => p.Add("0 3 * * *", new SomePulseCommand(), "Rollup")
                                .Add("0 4 * * *", new SomePulseEvent(), "rollup"));
        }

        [Test]
        public void AnExplicitNameThatCollidesWithADerivedNameShouldBeRejected()
        {
            var derived = Extensions.Pulse.PulseScheduleName.For(typeof(SomePulseCommand), "0 3 * * *");

            var exception = BuildAndCatch(p => p.Add("0 3 * * *", new SomePulseCommand())
                                                .Add("0 4 * * *", new SomePulseEvent(), derived));

            exception.Message.ShouldContain("collided with the name derived from");
        }

        [Test]
        public void TheClashingNameShouldBeNamedInTheMessage()
        {
            var exception = BuildAndCatch(p => p.Add("0 3 * * *", new SomePulseCommand(), "rollup")
                                                .Add("0 4 * * *", new SomePulseEvent(), "rollup"));

            exception.Message.ShouldContain("rollup");
        }

        [Test]
        public void DistinctNamesShouldBeAccepted()
        {
            var schedules = new PulseScheduleCollection();
            schedules.Add("0 3 * * *", new SomePulseCommand())
                     .Add("0 4 * * *", new SomePulseCommand())
                     .Add("0 3 * * *", new SomePulseEvent())
                     .Add("0 3 * * *", new SomePulseCommand(), "explicitly-named");

            schedules.Build().Length.ShouldBe(4);
        }
    }
}
