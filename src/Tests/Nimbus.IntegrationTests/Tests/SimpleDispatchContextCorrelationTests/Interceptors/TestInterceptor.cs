using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Interceptors.Inbound;
using Nimbus.PropertyInjection;

namespace Nimbus.IntegrationTests.Tests.SimpleDispatchContextCorrelationTests.Interceptors
{
    public class TestInterceptor : InboundInterceptor, IRequireDispatchContext
    {
        internal static readonly List<IDispatchContext> DispatchContexts = new List<IDispatchContext>();
        internal static readonly List<BrokeredMessage> BrokeredMessages = new List<BrokeredMessage>();

        public IDispatchContext DispatchContext { get; set; }
        public object BrokeredMessage { get; set; }

        public override async Task OnCommandHandlerExecuting<TBusCommand>(TBusCommand busCommand, BrokeredMessage brokeredMessage)
        {
            DispatchContexts.Add(DispatchContext);
            BrokeredMessages.Add(brokeredMessage);
        }

        public static void Clear()
        {
            DispatchContexts.Clear();
        }
    }
}