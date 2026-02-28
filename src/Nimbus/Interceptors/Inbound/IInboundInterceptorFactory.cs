using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;
using Nimbus.InfrastructureContracts.DependencyResolution;

namespace Nimbus.Interceptors.Inbound
{
    internal interface IInboundInterceptorFactory
    {
        IInboundInterceptor[] CreateInterceptors(IDependencyResolverScope scope, object handler, object message, NimbusMessage nimbusMessage);
    }
}