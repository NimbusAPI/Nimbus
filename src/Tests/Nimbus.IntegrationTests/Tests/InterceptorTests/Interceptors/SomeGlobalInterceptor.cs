using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Infrastructure;
using Nimbus.Interceptors.Inbound;
using Nimbus.Tests.Common;

namespace Nimbus.IntegrationTests.Tests.InterceptorTests.Interceptors
{
    public class SomeGlobalInterceptor : InboundInterceptor
    {
        public override async Task OnCommandHandlerExecuting<TBusCommand>(TBusCommand busCommand, NimbusMessage brokeredMessage)
        {
            MethodCallCounter.RecordCall<SomeGlobalInterceptor>(h => h.OnCommandHandlerExecuting(busCommand, brokeredMessage));
        }

        public override async Task OnCommandHandlerSuccess<TBusCommand>(TBusCommand busCommand, NimbusMessage brokeredMessage)
        {
            MethodCallCounter.RecordCall<SomeGlobalInterceptor>(h => h.OnCommandHandlerSuccess(busCommand, brokeredMessage));
        }

        public override async Task OnCommandHandlerError<TBusCommand>(TBusCommand busCommand, NimbusMessage brokeredMessage, Exception exception)
        {
            MethodCallCounter.RecordCall<SomeGlobalInterceptor>(h => h.OnCommandHandlerError(busCommand, brokeredMessage, exception));
        }
    }
}