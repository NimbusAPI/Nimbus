using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.NimbusMessageFactoryTests
{
    [TestFixture]
    internal class WhenCreatingANewMessageWithContent : GivenANimbusMessageFactory
    {
        private NimbusMessage _message;

        protected override async Task When()
        {
            _message = await Subject.Create("someQueue", new TestMessage());
        }

        [Test]
        public void TheCorrelationIdShouldNotBeEmpty()
        {
            _message.CorrelationId.ShouldNotBe(Guid.Empty);
        }

        [Test]
        public void TheCorrelationIdShouldNotBeTheMessageId()
        {
            _message.CorrelationId.ShouldNotBe(_message.MessageId);
        }

        [Test]
        public void ThenTheMessageTypeShouldBeSetToTheFullNameOfTheSerializedContent()
        {
            _message.SafelyGetBodyTypeNameOrDefault().ShouldBe(typeof (TestMessage).FullName);
        }

        [Test]
        public void ThenTheReplyToAddressShouldBeSetToTheSenderAddress()
        {
            _message.ReplyTo.ShouldBe(ReplyQueueNameSetting.Value);
        }

        [DataContract]
        public class TestMessage
        {
        }
    }
}