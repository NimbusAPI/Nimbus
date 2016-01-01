using Nimbus.DependencyResolution;
using Nimbus.Infrastructure;

namespace Nimbus.Interceptors.Inbound
{
    internal interface IInboundInterceptorFactory
    {
        IInboundInterceptor[] CreateInterceptors(IDependencyResolverScope scope, object handler, object message, NimbusMessage nimbusMessage);
    }
}