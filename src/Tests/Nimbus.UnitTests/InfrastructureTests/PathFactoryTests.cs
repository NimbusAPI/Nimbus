using Nimbus.Infrastructure;
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
            var pathFactory = PathFactory.CreateWithNoPrefix();
            var path = pathFactory.TopicPathFor(typeof(MyEscapingTestMessages.EscapingTestMessage));
            path.ShouldNotContain("+");
        }

        [Test]
        public void WhenCreatingAQueue_WeShouldConvertToLowerCase()
        {
            var pathFactory = PathFactory.CreateWithNoPrefix();
            var path = pathFactory.TopicPathFor(typeof(MyEscapingTestMessages.EscapingTestMessage));
            var expectedName = "t." + typeof(MyEscapingTestMessages.EscapingTestMessage).FullName.Replace("+", ".").ToLower();
            path.ShouldBe(expectedName);
        }

        [Test]
        public void WhenCreatingAQueueForAGenericType_WeShouldStripOutBackticks()
        {
            var pathFactory = PathFactory.CreateWithNoPrefix();
            var path = pathFactory.QueuePathFor(typeof(MyCommand<string>));
            path.ShouldNotContain("`");
        }

        [Test]
        public void WhenCreatingAQueueForAGenericType_WeShouldShortenTheGenericTypeArgumentName()
        {
            var pathFactory = PathFactory.CreateWithNoPrefix();
            var path = pathFactory.QueuePathFor(typeof(MyCommand<string>));
            path.ShouldBe("q.nimbus.unittests.infrastructuretests.messagecontracts.mycommand.1-string");
        }

        [Test]
        public void WhenCreatingASubscriptionForATypeASpaceShouldBeConvertedToTheSanitizeCharacter()
        {
            var pathFactory = PathFactory.CreateWithNoPrefix();
            var subscriptionName = pathFactory.SubscriptionNameFor("My App", "App server", typeof(MyEventWithALongName));
            subscriptionName.ShouldBe("my.app.app.server.myeventwithalongname");
        }

        [Test]
        public void WhenCreatingAQueuePathWithAGlobalPrefix_ThePathShouldStartWithThePrefix()
        {
            var pathFactory = PathFactory.CreateWithPrefix("testprefix");
            var path = pathFactory.QueuePathFor(typeof(SimpleCommand));
            path.ShouldBe("testprefix.q.nimbus.unittests.infrastructuretests.messagecontracts.simplecommand");
        }

        [Test]
        public void WhenCreatingATopicPathWithAGlobalPrefix_ThePathShouldStartWithThePrefix()
        {
            var pathFactory = PathFactory.CreateWithPrefix("testprefix");
            var path = pathFactory.TopicPathFor(typeof(SimpleEvent));
            path.ShouldBe("testprefix.t.nimbus.unittests.infrastructuretests.messagecontracts.simpleevent");
        }

        [Test]
        public void WhenCreatingAnInputQueuePathWithAGlobalPrefix_ThePathShouldStartWithThePrefix()
        {
            var pathFactory = PathFactory.CreateWithPrefix("testprefix");
            var path = pathFactory.InputQueuePathFor("My Application", "My Instance");
            path.ShouldBe("testprefix.inputqueue.my.application.my.instance");
        }

        [Test]
        public void WhenCreatingANameForACompetingSubscriptionWithAGlobalPrefix_ThePathShouldNotStartWithThePrefix()
        {
            var pathFactory = PathFactory.CreateWithPrefix("testprefix");
            var subscriptionName = pathFactory.SubscriptionNameFor("My Application", typeof(SimpleEvent));
            subscriptionName.ShouldBe("my.application.simpleevent");
        }

        [Test]
        public void WhenCreatingANameForAMulticastSubscriptionWithAGlobalPrefix_ThePathShouldNotStartWithThePrefix()
        {
            var pathFactory = PathFactory.CreateWithPrefix("testprefix");
            var subscriptionName = pathFactory.SubscriptionNameFor("My Application", "My Instance", typeof(SimpleEvent));
            subscriptionName.ShouldBe("my.application.my.instance.simpleevent");
        }

        [Test]
        public void WhenCreatingAQueueForATypeWithAVeryLongName_WeShouldHaveAPathOfTheCorrectMaximumLength()
        {
            var prefix = new string('x', 230);
            var pathFactory = PathFactory.CreateWithPrefix(prefix);
            var path = pathFactory.QueuePathFor(typeof(SimpleCommand));

            path.Length.ShouldBe(PathFactory.MaxPathLength);

            var expectedFullPath = $"{prefix}.q.nimbus.unittests.infrastructuretests.messagecontracts.simplecommand";
            var expectedShortenedPath = PathFactory.Shorten(expectedFullPath, PathFactory.MaxPathLength);
            path.ShouldBe(expectedShortenedPath);
        }

        [Test]
        public void WhenCreatingATopicForATypeWithAVeryLongName_WeShouldHaveAPathOfTheCorrectMaximumLength()
        {
            var prefix = new string('x', 230);
            var pathFactory = PathFactory.CreateWithPrefix(prefix);

            var path = pathFactory.TopicPathFor(typeof(SimpleEvent));

            path.Length.ShouldBe(PathFactory.MaxPathLength);

            var expectedFullPath = $"{prefix}.t.nimbus.unittests.infrastructuretests.messagecontracts.simpleevent";
            var expectedShortenedPath = PathFactory.Shorten(expectedFullPath, PathFactory.MaxPathLength);
            path.ShouldBe(expectedShortenedPath);
        }

        [Test]
        public void WhenCreatingASubscriptionForATypeWeHaveAMaximumLengthOf50()
        {
            var pathFactory = PathFactory.CreateWithNoPrefix();
            var path = pathFactory.SubscriptionNameFor("MyLongApplicationName", "Appserver", typeof(MyEventWithALongName));
            path.Length.ShouldBe(50);
        }
    }
}