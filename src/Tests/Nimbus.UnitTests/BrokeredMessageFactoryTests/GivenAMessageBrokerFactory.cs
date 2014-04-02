using System;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
using NSubstitute;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.BrokeredMessageFactoryTests
{
    public class GivenAMessageBrokerFactory
    {
        [TestFixture]
        public class WhenCreatingANewMessageWithContent : SpecificationFor<BrokeredMessageFactory>
        {
            private IClock _clock;
            private BrokeredMessage _message;
            private ReplyQueueNameSetting _replyQueueNameSetting;

            public override BrokeredMessageFactory Given()
            {
                _clock = Substitute.For<IClock>();
                _replyQueueNameSetting = new ReplyQueueNameSetting(
                    new ApplicationNameSetting {Value = "TestApplication"},
                    new InstanceNameSetting {Value = "TestInstance"});
                return new BrokeredMessageFactory(_replyQueueNameSetting, _clock);
            }

            public override void When()
            {
                _message = Subject.Create(new TestMessage());
            }

            [Test]
            public void ThenTheMessageIdShouldBeParsableToGuid()
            {
                Guid.ParseExact(_message.MessageId, "N");
            }

            [Test]
            public void ThenTheCorrelationIdShouldBeParsableToGuid()
            {
                Guid.ParseExact(_message.CorrelationId, "N");
            }

            [Test]
            public void ThenTheCorrelationIdShouldBeTheMessageId()
            {
                _message.CorrelationId.ShouldBe(_message.MessageId);
            }

            [Test]
            public void ThenTheMessageTypeShouldBeSetToTheFullNameOfTheSerializedContent()
            {
                _message.SafelyGetBodyTypeNameOrDefault().ShouldBe(typeof(TestMessage).FullName);
            }

            [Test]
            public void ThenTheReplyToAddressShouldBeSetToTheSenderAddress()
            {
                _message.ReplyTo.ShouldBe(_replyQueueNameSetting.Value);
            }

            public class TestMessage
            {
                
            }
        }
    }
}