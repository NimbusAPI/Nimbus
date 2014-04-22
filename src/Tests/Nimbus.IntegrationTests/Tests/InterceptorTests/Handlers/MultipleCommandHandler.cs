using System.Threading.Tasks;
using Nimbus.IntegrationTests.Tests.InterceptorTests.Interceptors;
using Nimbus.IntegrationTests.Tests.InterceptorTests.MessageContracts;
using Nimbus.Interceptors;

namespace Nimbus.IntegrationTests.Tests.InterceptorTests.Handlers
{
    [Interceptor(typeof (SomeClassLevelInterceptor))]
    public class MultipleCommandHandler : SomeBaseCommandHandler
    {
        [Interceptor(typeof (SomeMethodLevelInterceptorForFoo))]
        public override async Task Handle(FooCommand busCommand)
        {
            MethodCallCounter.RecordCall<MultipleCommandHandler>(h => h.Handle(busCommand));
        }

        [Interceptor(typeof (SomeMethodLevelInterceptorForBar))]
        public override async Task Handle(BarCommand busCommand)
        {
            MethodCallCounter.RecordCall<MultipleCommandHandler>(h => h.Handle(busCommand));
        }
    }
}