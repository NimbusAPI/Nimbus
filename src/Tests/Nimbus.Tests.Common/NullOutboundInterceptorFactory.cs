using Nimbus.DependencyResolution;
using Nimbus.Interceptors.Outbound;

namespace Nimbus.Tests.Common
{
    public class NullOutboundInterceptorFactory : IOutboundInterceptorFactory
    {
        public IOutboundInterceptor[] CreateInterceptors(IDependencyResolverScope scope)
        {
            return new IOutboundInterceptor[0];
        }
    }
}