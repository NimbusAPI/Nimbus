﻿using Nimbus.MessageContracts;

namespace Nimbus.Tests.Integration.Tests.AuditingInterceptorTests.MessageTypes
{
    public class SomeMulticastRequest : IBusMulticastRequest<SomeMulticastRequest, SomeMulticastResponse>
    {
    }
}