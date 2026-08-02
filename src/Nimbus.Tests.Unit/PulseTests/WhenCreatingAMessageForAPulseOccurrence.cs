using System;
using Cronos;
using Nimbus.Extensions.Pulse;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.Tests.Unit.PulseTests
{
    /// <summary>
    ///     The configured message is a template. Pulse copies it per fire so that stamping PulseTime
    ///     can't race an in-flight send, and so in-process handlers can't see it change underneath them.
    /// </summary>
    [TestFixture]
    public class WhenCreatingAMessageForAPulseOccurrence
    {
        private static PulseScheduleEntry EntryFor(object message)
        {
            return new PulseScheduleEntry("some-schedule", CronExpression.Parse("0 3 * * *"), message, true);
        }

        [Test]
        public void TheMessageShouldNotBeTheConfiguredInstance()
        {
            var template = new SomePulseCommand();

            EntryFor(template).CreateMessage().ShouldNotBeSameAs(template);
        }

        [Test]
        public void EachOccurrenceShouldGetItsOwnInstance()
        {
            var entry = EntryFor(new SomePulseCommand());

            entry.CreateMessage().ShouldNotBeSameAs(entry.CreateMessage());
        }

        [Test]
        public void TheMessageShouldBeOfTheConfiguredType()
        {
            EntryFor(new SomePulseCommand()).CreateMessage().ShouldBeOfType<SomePulseCommand>();
        }

        [Test]
        public void FieldValuesShouldBeCarriedAcross()
        {
            var template = new SomePulseCommand {Label = "nightly"};

            ((SomePulseCommand) EntryFor(template).CreateMessage()).Label.ShouldBe("nightly");
        }

        [Test]
        public void StampingThePulseTimeShouldNotMutateTheTemplate()
        {
            var template = new SomePulseCommandThatKnowsItsPulseTime();
            var entry = EntryFor(template);

            var occurrence = (IPulseMessage) entry.CreateMessage();
            occurrence.PulseTime = DateTimeOffset.UtcNow;

            template.PulseTime.ShouldBe(default(DateTimeOffset));
        }

        [Test]
        public void MutatingOneOccurrenceShouldNotAffectAnother()
        {
            var entry = EntryFor(new SomePulseCommand {Label = "original"});

            var first = (SomePulseCommand) entry.CreateMessage();
            var second = (SomePulseCommand) entry.CreateMessage();
            first.Label = "changed";

            second.Label.ShouldBe("original");
        }

        /// <summary>
        ///     Documents a known limitation rather than a desirable behaviour: the copy is shallow, so
        ///     reference-typed properties are still shared with the template.
        /// </summary>
        [Test]
        public void ReferenceTypedPropertiesShouldStillBeSharedWithTheTemplate()
        {
            var tags = new[] {"nightly"};
            var entry = EntryFor(new SomePulseCommand {Tags = tags});

            ((SomePulseCommand) entry.CreateMessage()).Tags.ShouldBeSameAs(tags);
        }
    }
}
