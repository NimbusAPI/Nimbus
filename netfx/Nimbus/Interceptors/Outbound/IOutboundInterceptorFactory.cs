using Nimbus.DependencyResolution;
using Nimbus.Infrastructure;

namespace Nimbus.Interceptors.Outbound
{
    internal interface IOutboundInterceptorFactory
    {
        IOutboundInterceptor[] CreateInterceptors(IDependencyResolverScope scope, NimbusMessage nimbusMessage);
    }
}