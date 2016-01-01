using System;
using System.Threading.Tasks;
using Nimbus.Infrastructure;
using Nimbus.Interceptors.Inbound;
using Nimbus.Tests.Common;
using Nimbus.Tests.Common.TestUtilities;

namespace Nimbus.IntegrationTests.Tests.InterceptorTests.Interceptors
{
    public sealed class SomeMethodLevelInterceptorForFoo : InboundInterceptor
    {
        public override async Task OnCommandHandlerExecuting<TBusCommand>(TBusCommand busCommand, NimbusMessage nimbusMessage)
        {
            MethodCallCounter.RecordCall<SomeMethodLevelInterceptorForFoo>(h => h.OnCommandHandlerExecuting(busCommand, nimbusMessage));
        }

        public override async Task OnCommandHandlerSuccess<TBusCommand>(TBusCommand busCommand, NimbusMessage nimbusMessage)
        {
            MethodCallCounter.RecordCall<SomeMethodLevelInterceptorForFoo>(h => h.OnCommandHandlerSuccess(busCommand, nimbusMessage));
        }

        public override async Task OnCommandHandlerError<TBusCommand>(TBusCommand busCommand, NimbusMessage nimbusMessage, Exception exception)
        {
            MethodCallCounter.RecordCall<SomeMethodLevelInterceptorForFoo>(h => h.OnCommandHandlerError(busCommand, nimbusMessage, exception));
        }
    }
}