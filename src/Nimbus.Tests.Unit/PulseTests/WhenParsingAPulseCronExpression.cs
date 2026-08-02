using System;
using Cronos;
using Nimbus.Extensions.Pulse;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.Tests.Unit.PulseTests
{
    /// <summary>
    ///     Cronos has to be told whether an expression carries a seconds field and rejects the format it
    ///     wasn't told to expect, so Pulse picks the format from the field count.
    /// </summary>
    [TestFixture]
    public class WhenParsingAPulseCronExpression
    {
        private static readonly DateTimeOffset _threeAm = new DateTimeOffset(2026, 8, 2, 3, 0, 0, TimeSpan.Zero);

        [Test]
        public void AFiveFieldExpressionShouldBeMinuteResolution()
        {
            var cron = PulseCronExpression.Parse("0 3 * * *");

            cron.GetNextOccurrence(_threeAm, TimeZoneInfo.Utc).ShouldBe(_threeAm.AddDays(1));
        }

        [Test]
        public void ASixFieldExpressionShouldBeSecondResolution()
        {
            var cron = PulseCronExpression.Parse("*/30 * * * * *");

            cron.GetNextOccurrence(_threeAm, TimeZoneInfo.Utc).ShouldBe(_threeAm.AddSeconds(30));
        }

        [Test]
        public void AnEverySecondExpressionShouldBeAccepted()
        {
            var cron = PulseCronExpression.Parse("* * * * * *");

            cron.GetNextOccurrence(_threeAm, TimeZoneInfo.Utc).ShouldBe(_threeAm.AddSeconds(1));
        }

        [Test]
        public void ExtraWhitespaceShouldNotChangeTheDetectedFormat()
        {
            var cron = PulseCronExpression.Parse("  */30   * * * * *  ");

            cron.GetNextOccurrence(_threeAm, TimeZoneInfo.Utc).ShouldBe(_threeAm.AddSeconds(30));
        }

        [Test]
        public void AMalformedExpressionShouldStillBeRejected()
        {
            Should.Throw<CronFormatException>(() => PulseCronExpression.Parse("not a cron expression"));
        }

        [Test]
        public void TooManyFieldsShouldBeRejected()
        {
            Should.Throw<CronFormatException>(() => PulseCronExpression.Parse("* * * * * * *"));
        }

        [Test]
        public void FieldCountShouldDriveTheFormatRatherThanTheContent()
        {
            // "0 0 * * *" is daily at midnight; "0 0 0 * * *" is the same instant expressed with seconds.
            var fiveField = PulseCronExpression.Parse("0 0 * * *");
            var sixField = PulseCronExpression.Parse("0 0 0 * * *");

            fiveField.GetNextOccurrence(_threeAm, TimeZoneInfo.Utc)
                     .ShouldBe(sixField.GetNextOccurrence(_threeAm, TimeZoneInfo.Utc));
        }

        [Test]
        public void ASixFieldScheduleShouldGetADistinctName()
        {
            var everyMinute = PulseScheduleName.For(typeof(SomePulseCommand), "* * * * *");
            var everySecond = PulseScheduleName.For(typeof(SomePulseCommand), "* * * * * *");

            everySecond.ShouldNotBe(everyMinute);
        }
    }
}
