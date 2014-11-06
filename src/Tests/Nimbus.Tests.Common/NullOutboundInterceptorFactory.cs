using Microsoft.ServiceBus.Messaging;
using Nimbus.DependencyResolution;
using Nimbus.Interceptors.Outbound;

namespace Nimbus.Tests.Common
{
    public class NullOutboundInterceptorFactory : IOutboundInterceptorFactory
    {
        public IOutboundInterceptor[] CreateInterceptors(IDependencyResolverScope scope, BrokeredMessage message)
        {
            return new IOutboundInterceptor[0];
        }
    }
}