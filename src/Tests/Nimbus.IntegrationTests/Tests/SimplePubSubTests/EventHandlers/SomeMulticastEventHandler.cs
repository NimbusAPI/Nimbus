using System;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.SimplePubSubTests.MessageContracts;
using Nimbus.PropertyInjection;
using Nimbus.Tests.Common;
using Nimbus.Tests.Common.TestUtilities;

#pragma warning disable 4014

namespace Nimbus.IntegrationTests.Tests.SimplePubSubTests.EventHandlers
{
    public class SomeMulticastEventHandler : IHandleMulticastEvent<SomeEventWeOnlyHandleViaMulticast>, IHandleMulticastEvent<SomeEventWeHandleViaMulticastAndCompetition>, IRequireBusId
    {
        public async Task Handle(SomeEventWeOnlyHandleViaMulticast busEvent)
        {
            MethodCallCounter.ForInstance(BusId).RecordCall<SomeMulticastEventHandler>(h => h.Handle(busEvent));
        }

        public async Task Handle(SomeEventWeHandleViaMulticastAndCompetition busEvent)
        {
            MethodCallCounter.ForInstance(BusId).RecordCall<SomeMulticastEventHandler>(h => h.Handle(busEvent));
        }

        public Guid BusId { get; set; }
    }
}