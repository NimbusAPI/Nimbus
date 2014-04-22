using System.Threading.Tasks;
using Nimbus.Handlers;
using Nimbus.IntegrationTests.Tests.InterceptorTests.Interceptors;
using Nimbus.IntegrationTests.Tests.InterceptorTests.MessageContracts;
using Nimbus.Interceptors;
using Nimbus.Interceptors.Inbound;

namespace Nimbus.IntegrationTests.Tests.InterceptorTests.Handlers
{
    [Interceptor(typeof (SomeBaseClassLevelInterceptor))]
    public abstract class SomeBaseCommandHandler : IHandleCommand<FooCommand>, IHandleCommand<BarCommand>
    {
        [Interceptor(typeof (SomeBaseMethodLevelInterceptorForFoo))]
        public abstract Task Handle(FooCommand busCommand);

        [Interceptor(typeof (SomeBaseMethodLevelInterceptorForBar))]
        public abstract Task Handle(BarCommand busCommand);
    }
}