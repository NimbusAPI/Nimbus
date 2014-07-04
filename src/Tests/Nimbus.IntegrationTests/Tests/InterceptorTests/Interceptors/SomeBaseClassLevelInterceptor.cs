using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Interceptors.Inbound;
using Nimbus.Tests.Common;

namespace Nimbus.IntegrationTests.Tests.InterceptorTests.Interceptors
{
    public class SomeBaseClassLevelInterceptor : InboundInterceptor
    {
        public override async Task OnCommandHandlerExecuting<TBusCommand>(TBusCommand busCommand, BrokeredMessage brokeredMessage)
        {
            MethodCallCounter.RecordCall<SomeBaseClassLevelInterceptor>(h => h.OnCommandHandlerExecuting(busCommand, brokeredMessage));
        }

        public override async Task OnCommandHandlerSuccess<TBusCommand>(TBusCommand busCommand, BrokeredMessage brokeredMessage)
        {
            MethodCallCounter.RecordCall<SomeBaseClassLevelInterceptor>(h => h.OnCommandHandlerSuccess(busCommand, brokeredMessage));
        }

        public override async Task OnCommandHandlerError<TBusCommand>(TBusCommand busCommand, BrokeredMessage brokeredMessage, Exception exception)
        {
            MethodCallCounter.RecordCall<SomeBaseClassLevelInterceptor>(h => h.OnCommandHandlerError(busCommand, brokeredMessage, exception));
        }
    }
}