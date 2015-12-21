using Nimbus.DependencyResolution;
using Nimbus.Interceptors.Inbound;

namespace Nimbus.Tests.Common.Stubs
{
    public class NullInboundInterceptorFactory : IInboundInterceptorFactory
    {
        public IInboundInterceptor[] CreateInterceptors(IDependencyResolverScope scope, object handler, object message, NimbusMessage nimbusMessage)
        {
            return new IInboundInterceptor[0];
        }
    }
}