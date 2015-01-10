using System.IO;
using Nimbus.Infrastructure;
using Nimbus.UnitTests.BatchSendingTests;
using Nimbus.UnitTests.InfrastructureTests.MessageContracts;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.InfrastructureTests
{
    [TestFixture]
    public class PathFactoryTests
    {
        private PathGenerator _pathGenerator;

        [SetUp]
        public void Setup()
        {
            _pathGenerator = new PathGenerator();
        }

        [Test]
        public void WhenCreatingAQueueForANestedType_WeShouldStripOutPlusSigns()
        {
            var pathName = _pathGenerator.TopicPathFor(typeof(MyEscapingTestMessages.EscapingTestMessage));
            pathName.ShouldNotContain("+");
        }

        [Test]
        public void WhenCreatingAQueue_WeShouldConvertToLowerCase()
        {
            var pathName = _pathGenerator.TopicPathFor(typeof(MyEscapingTestMessages.EscapingTestMessage));

            var expectedName = "t." +
                               typeof (MyEscapingTestMessages.EscapingTestMessage).FullName.Replace("+", ".").ToLower();

            pathName.ShouldBe(expectedName);
        }

        [Test]
        public void WhenCreatingAQueueForAGenericType_WeShouldStripOutBackticks()
        {
            var pathName = _pathGenerator.QueuePathFor(typeof(MyCommand<string>));
            pathName.ShouldNotContain("`");
        }

        [Test]
        public void WhenCreatingAQueueForAGenericType_WeShouldShortenTheGenericTypeArgumentName()
        {
            var pathName = _pathGenerator.QueuePathFor(typeof(MyCommand<string>));

            var expected = "q.nimbus.unittests.infrastructuretests.messagecontracts.mycommand.1-string";

            pathName.ShouldBe(expected);
        }

        [Test]
        public void WhenCreatingASubscriptionForATypeWeHaveAMaximumLengthOf50()
        {
            var pathName = _pathGenerator.SubscriptionNameFor("MyLongApplicationName", "Appserver", typeof(MyEventWithALongName));
            pathName.Length.ShouldBe(50);
        }
    }
}