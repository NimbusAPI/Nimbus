using Nimbus.DependencyResolution;
using Nimbus.Interceptors;
using Nimbus.Interceptors.Inbound;

namespace Nimbus.UnitTests
{
    public class NullInboundInterceptorFactory : IInboundInterceptorFactory
    {
        public IInboundInterceptor[] CreateInterceptors(IDependencyResolverScope scope, object handler, object message)
        {
            return new IInboundInterceptor[0];
        }
    }
}