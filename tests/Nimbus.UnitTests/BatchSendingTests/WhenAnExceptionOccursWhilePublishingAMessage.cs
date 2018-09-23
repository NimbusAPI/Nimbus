using System;
using System.Threading.Tasks;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.Dispatching;
using Nimbus.Infrastructure.Events;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.Infrastructure.Routing;
using Nimbus.Tests.Common.Stubs;
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

            var transport = Substitute.For<INimbusTransport>();
            transport.GetTopicSender(Arg.Any<string>()).Returns(ci => _nimbusMessageSender);

            // simulate an exception being thrown when sending a message
            _nimbusMessageSender.Send(Arg.Any<NimbusMessage>()).Returns(ci => { throw new Exception(); });

            var clock = new SystemClock();
            var pathFactory = new PathFactory(new GlobalPrefixSetting());
            var replyQueueNameSetting = new ReplyQueueNameSetting(
                new ApplicationNameSetting { Value = "TestApplication" },
                new InstanceNameSetting { Value = "TestInstance" },
                pathFactory);

            var nimbusMessageFactory = new NimbusMessageFactory(new DefaultMessageTimeToLiveSetting(),
                                                                replyQueueNameSetting,
                                                                clock,
                                                                new DispatchContextManager());
            var logger = Substitute.For<ILogger>();
            var knownMessageTypeVerifier = Substitute.For<IKnownMessageTypeVerifier>();
            var router = new DestinationPerMessageTypeRouter();
            var dependencyResolver = new NullDependencyResolver();
            var outboundInterceptorFactory = new NullOutboundInterceptorFactory();
            var busCommandSender = new BusEventSender(dependencyResolver,
                                                      knownMessageTypeVerifier,
                                                      logger,
                                                      nimbusMessageFactory,
                                                      transport,
                                                      outboundInterceptorFactory,
                                                      pathFactory, router);
            return Task.FromResult(busCommandSender);
        }

        protected override async Task When()
        {
        }

        [Test]
        public async Task ThenTheExceptionShouldBeThrownBack()
        {
            var testEvent = new FooEvent();

            Assert.That(() => Subject.Publish(testEvent), Throws.TypeOf<Exception>());
        }
    }
}