using Nimbus.Infrastructure;
using Nimbus.MessageContracts;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.Tests.InfrastructureTests
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



        
    }

    public class MyCommand<T>
    {
        public T Metadata { get; set; }
    }

    public class MyEscapingTestMessages
    {
        public class EscapingTestMessage : IBusEvent
        {
            
        }
    }
}