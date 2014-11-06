using Microsoft.ServiceBus.Messaging;
using Nimbus.DependencyResolution;

namespace Nimbus.Interceptors.Inbound
{
    internal interface IInboundInterceptorFactory
    {
        IInboundInterceptor[] CreateInterceptors(IDependencyResolverScope scope, object handler, object message, BrokeredMessage brokeredMessage);
    }
}