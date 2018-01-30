using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.SimpleCommandSendingTests.MessageContracts;
using Nimbus.PropertyInjection;
using Nimbus.Tests.Common;
using Nimbus.Tests.Common.TestUtilities;

namespace Nimbus.IntegrationTests.Tests.SimpleCommandSendingTests.CommandHandlers
{
    public class SomeOtherCommandHandler : IHandleCommand<SomeOtherCommand>, IRequireDispatchContext, IRequireMessageProperties, IRequireBusId
    {
        private static IDictionary<string, object> _receivedMessageProperties;
        private static IDispatchContext _receivedDispatchContext;

        public IDispatchContext DispatchContext { get; set; }
        public IDictionary<string, object> MessageProperties { get; set; }
        public Guid BusId { get; set; }

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

        public async Task Handle(SomeOtherCommand busCommand)
        {
            _receivedMessageProperties = MessageProperties;
            _receivedDispatchContext = DispatchContext;
            MethodCallCounter.ForInstance(BusId).RecordCall<SomeOtherCommandHandler>(ch => ch.Handle(busCommand));
        }
    }
}