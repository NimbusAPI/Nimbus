using Nimbus.DependencyResolution;
using Nimbus.Interceptors.Outbound;

namespace Nimbus.Tests.Common.Stubs
{
    public class NullOutboundInterceptorFactory : IOutboundInterceptorFactory
    {
        public IOutboundInterceptor[] CreateInterceptors(IDependencyResolverScope scope, NimbusMessage message)
        {
            return new IOutboundInterceptor[0];
        }
    }
}