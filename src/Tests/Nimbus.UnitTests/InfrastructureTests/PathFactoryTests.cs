﻿using Nimbus.Infrastructure;
using Nimbus.UnitTests.InfrastructureTests.MessageContracts;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.InfrastructureTests
{
    [TestFixture]
    public class PathFactoryTests
    {
        [Test]
        public void WhenCreatingAQueueForANestedType_WeShouldStripOutPlusSigns()
        {
            var pathName = PathFactory.TopicPathFor(typeof (MyEscapingTestMessages.EscapingTestMessage));
            pathName.ShouldNotContain("+");
        }

        [Test]
        public void WhenCreatingAQueue_WeShouldConvertToLowerCase()
        {
            var pathName = PathFactory.TopicPathFor(typeof (MyEscapingTestMessages.EscapingTestMessage));

            var expectedName = "t." +
                               typeof (MyEscapingTestMessages.EscapingTestMessage).FullName.Replace("+", ".").ToLower();

            pathName.ShouldBe(expectedName);
        }

        [Test]
        public void WhenCreatingAQueueForAGenericType_WeShouldStripOutBackticks()
        {
            var pathName = PathFactory.QueuePathFor(typeof (MyCommand<string>));
            pathName.ShouldNotContain("`");
        }

        [Test]
        public void WhenCreatingAQueueForAGenericType_WeShouldShortenTheGenericTypeArgumentName()
        {
            var pathName = PathFactory.QueuePathFor(typeof (MyCommand<string>));

            var expected = "q.nimbus.unittests.infrastructuretests.messagecontracts.mycommand.1-string";

            pathName.ShouldBe(expected);
        }

        [Test]
        public void WhenCreatingASubscriptionForATypeWeHaveAMaximumLengthOf50()
        {
            var pathName = PathFactory.SubscriptionNameFor("MyLongApplicationName", "Appserver", typeof (MyEventWithALongName));
            pathName.Length.ShouldBe(50);
        }

        [Test]
        public void WhenCreatingASubscriptionForATypeASpaceShouldBeConvertedToTheSanitizeCharacter()
        {
            var pathName = PathFactory.SubscriptionNameFor("My App", "App server", typeof(MyEventWithALongName));
            pathName.ShouldBe("my.app.app.server.myeventwithalongname");
        }
    }
}