using Nimbus.DependencyResolution;
using Nimbus.Infrastructure;
using Nimbus.Interceptors.Inbound;

namespace Nimbus.Tests.Common
{
    public class NullInboundInterceptorFactory : IInboundInterceptorFactory
    {
        public IInboundInterceptor[] CreateInterceptors(IDependencyResolverScope scope, object handler, object message, NimbusMessage brokeredMessage)
        {
            return new IInboundInterceptor[0];
        }
    }
}