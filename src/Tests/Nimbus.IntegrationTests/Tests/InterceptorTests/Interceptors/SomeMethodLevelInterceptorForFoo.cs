using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Interceptors;
using Nimbus.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.InterceptorTests.Interceptors
{
    public sealed class SomeMethodLevelInterceptorForFoo : MessageInterceptor
    {
        public override async Task OnCommandHandlerExecuting(IBusCommand busCommand, BrokeredMessage brokeredMessage)
        {
            MethodCallCounter.RecordCall<SomeMethodLevelInterceptorForFoo>(h => h.OnCommandHandlerExecuting(busCommand, brokeredMessage));
        }

        public override async Task OnCommandHandlerSuccess(IBusCommand busCommand, BrokeredMessage brokeredMessage)
        {
            MethodCallCounter.RecordCall<SomeMethodLevelInterceptorForFoo>(h => h.OnCommandHandlerSuccess(busCommand, brokeredMessage));
        }

        public override async Task OnCommandHandlerError(IBusCommand busCommand, BrokeredMessage brokeredMessage, Exception exception)
        {
            MethodCallCounter.RecordCall<SomeMethodLevelInterceptorForFoo>(h => h.OnCommandHandlerError(busCommand, brokeredMessage, exception));
        }
    }
}