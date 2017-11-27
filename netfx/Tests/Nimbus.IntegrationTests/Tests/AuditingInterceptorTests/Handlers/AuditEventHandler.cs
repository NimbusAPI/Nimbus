﻿using System;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.MessageContracts.ControlMessages;
using Nimbus.PropertyInjection;
using Nimbus.Tests.Common.TestUtilities;
using Serilog;

namespace Nimbus.IntegrationTests.Tests.AuditingInterceptorTests.Handlers
{
    public class AuditEventHandler : IHandleCompetingEvent<AuditEvent>, IRequireBusId
    {
        public async Task Handle(AuditEvent busEvent)
        {
            MethodCallCounter.ForInstance(BusId).RecordCall<AuditEventHandler>(h => h.Handle(busEvent));
            Log.Debug("Received audit message {@AuditMessage}", busEvent);
        }

        public Guid BusId { get; set; }
    }
}