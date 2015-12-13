using Microsoft.ServiceBus.Messaging;
using Nimbus.DependencyResolution;
using Nimbus.Infrastructure;
using Nimbus.Interceptors.Outbound;

namespace Nimbus.Tests.Common
{
    public class NullOutboundInterceptorFactory : IOutboundInterceptorFactory
    {
        public IOutboundInterceptor[] CreateInterceptors(IDependencyResolverScope scope, NimbusMessage message)
        {
            return new IOutboundInterceptor[0];
        }
    }
}