﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.SimpleCommandSendingTests.MessageContracts;
using Nimbus.PropertyInjection;
using Nimbus.Tests.Common;

namespace Nimbus.IntegrationTests.Tests.SimpleCommandSendingTests.CommandHandlers
{
    public class SomeOtherCommandHandler : IHandleCommand<SomeOtherCommand>, IRequireDispatchContext, IRequireMessageProperties
    {
        private static IDictionary<string, object> _receivedMessageProperties;
        private static IDispatchContext _receivedDispatchContext;

        public IDispatchContext DispatchContext { get; set; }
        public IDictionary<string, object> MessageProperties { get; set; }

        public static IDictionary<string, object> ReceivedMessageProperties
        {
            get { return _receivedMessageProperties; }
        }

        public static IDispatchContext ReceivedDispatchContext
        {
            get { return _receivedDispatchContext; }
        }

        public static void Clear()
        {
            _receivedMessageProperties = null;
            _receivedDispatchContext = null;
        }

#pragma warning disable 4014 // Because this call is not awaited, execution of the current method continues before the call is completed.
		  public async Task Handle(SomeOtherCommand busCommand)
        {
            _receivedMessageProperties = MessageProperties;
            _receivedDispatchContext = DispatchContext;
            MethodCallCounter.RecordCall<SomeOtherCommandHandler>(ch => ch.Handle(busCommand));
        }
    }
}