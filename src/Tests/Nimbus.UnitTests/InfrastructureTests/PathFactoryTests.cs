using Nimbus.Infrastructure;
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

            var expected = "q.nimbus.tests.infrastructuretests.mycommand.1-string";

            pathName.ShouldBe(expected);
        }
    }
}