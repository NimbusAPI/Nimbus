using Nimbus.DependencyResolution;

namespace Nimbus.Interceptors
{
    internal interface IInterceptorFactory
    {
        IMessageInterceptor[] CreateInterceptors(IDependencyResolverScope scope, object handler, object message);
    }
}