using Nimbus.InfrastructureContracts;
using Nimbus.IntegrationTests.Tests.ThroughputTests.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.ThroughputTests.EventHandlers
{
    /// <summary>
    ///     This class needs to exist so that our bus knows to subscribe to these types of message. That's all. It's not
    ///     supposed to do anything.
    /// </summary>
    public class GreedyHandler : IHandleMulticastEvent<FooEvent>,
                                 IHandleMulticastEvent<BarEvent>,
                                 IHandleMulticastEvent<BazEvent>,
                                 IHandleMulticastEvent<QuxEvent>,
                                 IHandleCompetingEvent<FooEvent>,
                                 IHandleCompetingEvent<BarEvent>,
                                 IHandleCompetingEvent<BazEvent>,
                                 IHandleCompetingEvent<QuxEvent>,
                                 IHandleCommand<FooCommand>,
                                 IHandleCommand<BarCommand>,
                                 IHandleCommand<BazCommand>,
                                 IHandleCommand<QuxCommand>
    {
        public void Handle(FooEvent busEvent)
        {
        }

        public void Handle(BarEvent busEvent)
        {
        }

        public void Handle(BazEvent busEvent)
        {
        }

        public void Handle(QuxEvent busEvent)
        {
        }

        public void Handle(FooCommand busCommand)
        {
        }

        public void Handle(BarCommand busCommand)
        {
        }

        public void Handle(BazCommand busCommand)
        {
        }

        public void Handle(QuxCommand busCommand)
        {
        }
    }
}