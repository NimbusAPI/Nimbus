using Nimbus.DependencyResolution;
using Nimbus.Interceptors;

namespace Nimbus.UnitTests
{
    public class NullInterceptorFactory : IInterceptorFactory
    {
        public IMessageInterceptor[] CreateInterceptors(IDependencyResolverScope scope, object handler, object message)
        {
            return new IMessageInterceptor[0];
        }
    }
}