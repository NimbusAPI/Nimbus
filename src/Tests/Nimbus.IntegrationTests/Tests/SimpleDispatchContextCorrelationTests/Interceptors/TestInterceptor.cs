using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Interceptors.Inbound;

namespace Nimbus.IntegrationTests.Tests.SimpleDispatchContextCorrelationTests.Interceptors
{
    public class TestInterceptor : InboundInterceptor
    {
        internal static readonly List<string> ReceivedCorrelationIds = new List<string>();

        public override async Task OnCommandHandlerExecuting<TBusCommand>(TBusCommand busCommand, BrokeredMessage brokeredMessage)
        {
            ReceivedCorrelationIds.Add(brokeredMessage.CorrelationId);
        }
    }
}