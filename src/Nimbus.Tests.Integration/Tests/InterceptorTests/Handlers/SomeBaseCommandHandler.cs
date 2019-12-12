using System.Threading.Tasks;
using Nimbus.InfrastructureContracts.Handlers;
using Nimbus.Interceptors.Inbound;
using Nimbus.Tests.Integration.Tests.InterceptorTests.Interceptors;
using Nimbus.Tests.Integration.Tests.InterceptorTests.MessageContracts;

namespace Nimbus.Tests.Integration.Tests.InterceptorTests.Handlers
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