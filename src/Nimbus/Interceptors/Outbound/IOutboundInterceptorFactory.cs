using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;
using Nimbus.InfrastructureContracts.DependencyResolution;

namespace Nimbus.Interceptors.Outbound
{
    internal interface IOutboundInterceptorFactory
    {
        IOutboundInterceptor[] CreateInterceptors(IDependencyResolverScope scope, NimbusMessage nimbusMessage);
    }
}