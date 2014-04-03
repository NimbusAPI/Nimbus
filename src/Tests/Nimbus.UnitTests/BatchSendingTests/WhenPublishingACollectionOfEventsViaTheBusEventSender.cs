using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.Events;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.MessageContracts;
using Nimbus.Serliazers;
using Nimbus.UnitTests.BatchSendingTests.MessageContracts;
using NSubstitute;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.BatchSendingTests
{
    [TestFixture]
    internal class WhenPublishingACollectionOfEventsViaTheBusEventSender : SpecificationForAsync<BusEventSender>
    {
        private INimbusMessageSender _nimbusMessageSender;

        protected override Task<BusEventSender> Given()
        {
            _nimbusMessageSender = Substitute.For<INimbusMessageSender>();

            var messagingFactory = Substitute.For<INimbusMessagingFactory>();
            messagingFactory.GetTopicSender(Arg.Any<string>()).Returns(ci => _nimbusMessageSender);

            var clock = new SystemClock();
            var serialzer = new DefaultSerializer();
            var replyQueueNameSetting = new ReplyQueueNameSetting(
                new ApplicationNameSetting { Value = "TestApplication" },
                new InstanceNameSetting { Value = "TestInstance" });
            var brokeredMessageFactory = new BrokeredMessageFactory(replyQueueNameSetting, serialzer, clock);
            var validEventTypes = new EventTypesSetting { Value = new[] { typeof(FooEvent), typeof(BarEvent), typeof(BazEvent) } };
            var logger = Substitute.For<ILogger>();

            var busCommandSender = new BusEventSender(messagingFactory, brokeredMessageFactory, validEventTypes, logger);
            return Task.FromResult(busCommandSender);
        }

        protected override async Task When()
        {
            var events = new IBusEvent[] {new FooEvent(), new BarEvent(), new BazEvent()};

            foreach (var e in events)
            {
                await Subject.Publish(e);
            }
        }

        [Test]
        public void TheEventSenderShouldHaveReceivedThreeCalls()
        {
            _nimbusMessageSender.ReceivedCalls().Count().ShouldBe(3);
        }
    }
}