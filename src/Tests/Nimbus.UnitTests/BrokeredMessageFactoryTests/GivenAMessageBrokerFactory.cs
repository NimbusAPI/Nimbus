using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Configuration.LargeMessages.Settings;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.BrokeredMessageServices;
using Nimbus.Infrastructure.BrokeredMessageServices.Compression;
using Nimbus.Infrastructure.BrokeredMessageServices.LargeMessages;
using NSubstitute;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.BrokeredMessageFactoryTests
{
    internal abstract class GivenAMessageBrokerFactory : SpecificationForAsync<BrokeredMessageFactory>
    {
        private IClock _clock;
        private ISerializer _serializer;
        private BrokeredMessage _message;
        private ReplyQueueNameSetting _replyQueueNameSetting;

        protected override async Task<BrokeredMessageFactory> Given()
        {
            _clock = Substitute.For<IClock>();
            _serializer = Substitute.For<ISerializer>();
            _replyQueueNameSetting = new ReplyQueueNameSetting(new ApplicationNameSetting {Value = "TestApplication"}, new InstanceNameSetting {Value = "TestInstance"});

            return new BrokeredMessageFactory(_replyQueueNameSetting,
                                              _serializer,
                                              new NullCompressor(),
                                              _clock,
                                              new UnsupportedLargeMessageBodyStore(),
                                              new MaxSmallMessageSizeSetting(),
                                              new MaxLargeMessageSizeSetting());
        }

        [TestFixture]
        public class WhenCreatingANewMessageWithContent : GivenAMessageBrokerFactory
        {
            protected override async Task When()
            {
                _message = await Subject.Create(new TestMessage());
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
                _message.SafelyGetBodyTypeNameOrDefault().ShouldBe(typeof (TestMessage).FullName);
            }

            [Test]
            public void ThenTheReplyToAddressShouldBeSetToTheSenderAddress()
            {
                _message.ReplyTo.ShouldBe(_replyQueueNameSetting.Value);
            }

            [DataContract]
            public class TestMessage
            {
            }
        }
    }
}