using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.SimplePubSubTests.MessageContracts;
using Nimbus.Tests.Common;
using Nimbus.Tests.Common.TestUtilities;

#pragma warning disable 4014

namespace Nimbus.IntegrationTests.Tests.SimplePubSubTests.EventHandlers
{
    public class SomeCompetingEventHandler : IHandleCompetingEvent<SomeEventWeOnlyHandleViaCompetition>, IHandleCompetingEvent<SomeEventWeHandleViaMulticastAndCompetition>
    {
        public async Task Handle(SomeEventWeOnlyHandleViaCompetition busEvent)
        {
            MethodCallCounter.RecordCall<SomeCompetingEventHandler>(h => h.Handle(busEvent));
        }

        public async Task Handle(SomeEventWeHandleViaMulticastAndCompetition busEvent)
        {
            MethodCallCounter.RecordCall<SomeCompetingEventHandler>(h => h.Handle(busEvent));
        }
    }
}