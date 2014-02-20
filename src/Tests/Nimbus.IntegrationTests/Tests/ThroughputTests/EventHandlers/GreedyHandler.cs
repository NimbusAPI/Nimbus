using System.Threading.Tasks;
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
        public async Task Handle(FooEvent busEvent)
        {
        }

        public async Task Handle(BarEvent busEvent)
        {
        }

        public async Task Handle(BazEvent busEvent)
        {
        }

        public async Task Handle(QuxEvent busEvent)
        {
        }

        public async Task Handle(FooCommand busCommand)
        {
        }

        public async Task Handle(BarCommand busCommand)
        {
        }

        public async Task Handle(BazCommand busCommand)
        {
        }

        public async Task Handle(QuxCommand busCommand)
        {
        }
    }
}