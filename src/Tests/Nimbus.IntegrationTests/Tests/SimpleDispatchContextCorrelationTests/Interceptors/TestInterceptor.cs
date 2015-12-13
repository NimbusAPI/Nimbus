using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Infrastructure;
using Nimbus.Interceptors.Inbound;
using Nimbus.PropertyInjection;

namespace Nimbus.IntegrationTests.Tests.SimpleDispatchContextCorrelationTests.Interceptors
{
    public class TestInterceptor : InboundInterceptor, IRequireDispatchContext
    {
        internal static readonly List<IDispatchContext> DispatchContexts = new List<IDispatchContext>();
        internal static readonly List<NimbusMessage> NimbusMessages = new List<NimbusMessage>();

        public IDispatchContext DispatchContext { get; set; }
        public object BrokeredMessage { get; set; }

        public override async Task OnCommandHandlerExecuting<TBusCommand>(TBusCommand busCommand, NimbusMessage brokeredMessage)
        {
            DispatchContexts.Add(DispatchContext);
            NimbusMessages.Add(brokeredMessage);
        }

        public static void Clear()
        {
            DispatchContexts.Clear();
        }
    }
}