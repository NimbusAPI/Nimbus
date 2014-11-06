using Microsoft.ServiceBus.Messaging;
using Nimbus.DependencyResolution;
using Nimbus.Interceptors.Inbound;

namespace Nimbus.Tests.Common
{
    public class NullInboundInterceptorFactory : IInboundInterceptorFactory
    {
        public IInboundInterceptor[] CreateInterceptors(IDependencyResolverScope scope, object handler, object message, BrokeredMessage brokeredMessage)
        {
            return new IInboundInterceptor[0];
        }
    }
}