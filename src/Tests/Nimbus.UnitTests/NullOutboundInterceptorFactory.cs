using Nimbus.DependencyResolution;
using Nimbus.Interceptors.Inbound;
using Nimbus.Interceptors.Outbound;

namespace Nimbus.UnitTests
{
    internal class NullOutboundInterceptorFactory : IOutboundInterceptorFactory
    {
        public IOutboundInterceptor[] CreateInterceptors(IDependencyResolverScope scope)
        {
            return new IOutboundInterceptor[0];
        }
    }
}