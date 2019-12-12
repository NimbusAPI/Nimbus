﻿using System;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts.Handlers;
using Nimbus.InfrastructureContracts.PropertyInjection;
using Nimbus.Tests.Common.TestUtilities;
using Nimbus.Tests.Integration.Tests.SimplePubSubTests.MessageContracts;

#pragma warning disable 4014

namespace Nimbus.Tests.Integration.Tests.SimplePubSubTests.EventHandlers
{
    public class SomeCompetingEventHandler : IHandleCompetingEvent<SomeEventWeOnlyHandleViaCompetition>,
                                             IHandleCompetingEvent<SomeEventWeHandleViaMulticastAndCompetition>,
                                             IRequireBusId
    {
        public async Task Handle(SomeEventWeOnlyHandleViaCompetition busEvent)
        {
            MethodCallCounter.ForInstance(BusId).RecordCall<SomeCompetingEventHandler>(h => h.Handle(busEvent));
        }

        public async Task Handle(SomeEventWeHandleViaMulticastAndCompetition busEvent)
        {
            MethodCallCounter.ForInstance(BusId).RecordCall<SomeCompetingEventHandler>(h => h.Handle(busEvent));
        }

        public Guid BusId { get; set; }
    }
}