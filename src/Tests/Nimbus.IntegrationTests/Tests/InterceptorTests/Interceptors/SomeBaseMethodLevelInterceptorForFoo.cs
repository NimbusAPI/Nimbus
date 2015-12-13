using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Infrastructure;
using Nimbus.Interceptors.Inbound;
using Nimbus.Tests.Common;

namespace Nimbus.IntegrationTests.Tests.InterceptorTests.Interceptors
{
    public class SomeBaseMethodLevelInterceptorForFoo : InboundInterceptor
    {
        public override async Task OnCommandHandlerExecuting<TBusCommand>(TBusCommand busCommand, NimbusMessage nimbusMessage)
        {
            MethodCallCounter.RecordCall<SomeBaseMethodLevelInterceptorForFoo>(h => h.OnCommandHandlerExecuting(busCommand, nimbusMessage));
        }

        public override async Task OnCommandHandlerSuccess<TBusCommand>(TBusCommand busCommand, NimbusMessage nimbusMessage)
        {
            MethodCallCounter.RecordCall<SomeBaseMethodLevelInterceptorForFoo>(h => h.OnCommandHandlerSuccess(busCommand, nimbusMessage));
        }

        public override async Task OnCommandHandlerError<TBusCommand>(TBusCommand busCommand, NimbusMessage nimbusMessage, Exception exception)
        {
            MethodCallCounter.RecordCall<SomeBaseMethodLevelInterceptorForFoo>(h => h.OnCommandHandlerError(busCommand, nimbusMessage, exception));
        }
    }
}