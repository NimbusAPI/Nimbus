using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.Commands;
using Nimbus.Infrastructure.Dispatching;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.Infrastructure.Routing;
using Nimbus.MessageContracts;
using Nimbus.Tests.Common.Stubs;
using Nimbus.UnitTests.BatchSendingTests.MessageContracts;
using NSubstitute;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.BatchSendingTests
{
    [TestFixture]
    internal class WhenSendingACollectionOfCommandsViaTheCommandSender : SpecificationForAsync<BusCommandSender>
    {
        private INimbusMessageSender _nimbusMessageSender;

        protected override Task<BusCommandSender> Given()
        {
            _nimbusMessageSender = Substitute.For<INimbusMessageSender>();

            var transport = Substitute.For<INimbusTransport>();
            transport.GetQueueSender(Arg.Any<string>()).Returns(ci => _nimbusMessageSender);

            var clock = new SystemClock();
            var pathFactory = new PathFactory(new GlobalPrefixSetting());
            var replyQueueNameSetting = new ReplyQueueNameSetting(
                new ApplicationNameSetting {Value = "TestApplication"},
                new InstanceNameSetting {Value = "TestInstance"},
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

            var busCommandSender = new BusCommandSender(dependencyResolver,
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
            var commands = new IBusCommand[] {new FooCommand(), new BarCommand(), new BazCommand()};

            foreach (var command in commands)
            {
                await Subject.Send(command);
            }
        }

        [Test]
        public void TheCommandSenderShouldHaveReceivedThreeCalls()
        {
            _nimbusMessageSender.ReceivedCalls().Count().ShouldBe(3);
        }
    }
}