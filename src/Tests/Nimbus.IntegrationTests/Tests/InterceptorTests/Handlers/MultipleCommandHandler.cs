using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.InterceptorTests.MessageContracts;
using Nimbus.Interceptors;

namespace Nimbus.IntegrationTests.Tests.InterceptorTests.Handlers
{
    [Interceptor(typeof (SomeClassLevelInterceptor))]
    public class MultipleCommandHandler : IHandleCommand<FooCommand>, IHandleCommand<BarCommand>
    {
        [Interceptor(typeof (SomeMethodLevelInterceptorForFoo))]
        public async Task Handle(FooCommand busCommand)
        {
            MethodCallCounter.RecordCall<MultipleCommandHandler>(h => h.Handle(busCommand));
        }

        [Interceptor(typeof (SomeMethodLevelInterceptorForBar))]
        public async Task Handle(BarCommand busCommand)
        {
            MethodCallCounter.RecordCall<MultipleCommandHandler>(h => h.Handle(busCommand));
        }
    }
}