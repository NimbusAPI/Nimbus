using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Configuration.LargeMessages.Settings;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.BrokeredMessageServices;
using Nimbus.Infrastructure.BrokeredMessageServices.Compression;
using Nimbus.Infrastructure.BrokeredMessageServices.LargeMessages;
using Nimbus.Infrastructure.BrokeredMessageServices.Serialization;
using Nimbus.Infrastructure.Dispatching;
using Nimbus.Infrastructure.Events;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.Infrastructure.Routing;
using Nimbus.Tests.Common;
using Nimbus.UnitTests.BatchSendingTests.MessageContracts;
using NSubstitute;
using NUnit.Framework;

namespace Nimbus.UnitTests.BatchSendingTests
{
    [TestFixture]
    internal class WhenAnExceptionOccursWhilePublishingAMessage : SpecificationForAsync<BusEventSender>
    {
        private INimbusMessageSender _nimbusMessageSender;

        protected override Task<BusEventSender> Given()
        {
            _nimbusMessageSender = Substitute.For<INimbusMessageSender>();

            var messagingFactory = Substitute.For<INimbusMessagingFactory>();
            messagingFactory.GetTopicSender(Arg.Any<string>()).Returns(ci => _nimbusMessageSender);

            // simulate an exception being thrown when sending a message
            _nimbusMessageSender.Send(Arg.Any<BrokeredMessage>()).Returns(ci => { throw new Exception(); });

            var clock = new SystemClock();
            var typeProvider = new TestHarnessTypeProvider(new[] { GetType().Assembly }, new[] { GetType().Namespace });
            var serializer = new DataContractSerializer(typeProvider);
            var replyQueueNameSetting = new ReplyQueueNameSetting(
                new ApplicationNameSetting { Value = "TestApplication" },
                new InstanceNameSetting { Value = "TestInstance" });
            var brokeredMessageFactory = new BrokeredMessageFactory(new DefaultMessageTimeToLiveSetting(),
                                                                    new MaxLargeMessageSizeSetting(),
                                                                    new MaxSmallMessageSizeSetting(),
                                                                    replyQueueNameSetting,
                                                                    clock,
                                                                    new NullCompressor(),
                                                                    new DispatchContextManager(),
                                                                    new UnsupportedLargeMessageBodyStore(),
                                                                    serializer,
                                                                    typeProvider);
            var logger = Substitute.For<ILogger>();
            var knownMessageTypeVerifier = Substitute.For<IKnownMessageTypeVerifier>();
            var router = new DestinationPerMessageTypeRouter();
            var dependencyResolver = new NullDependencyResolver();
            var outboundInterceptorFactory = new NullOutboundInterceptorFactory();
            var busCommandSender = new BusEventSender(brokeredMessageFactory,
                                                      dependencyResolver,
                                                      knownMessageTypeVerifier,
                                                      logger,
                                                      messagingFactory,
                                                      outboundInterceptorFactory,
                                                      router);
            return Task.FromResult(busCommandSender);
        }

        protected override async Task When(){}

        [Test]
        public async Task ThenTheExceptionShouldBeThrownBack()
        {
            Assert.That(async() => await Subject.Publish(new FooEvent()), Throws.TypeOf<Exception>());
        }
    }
}