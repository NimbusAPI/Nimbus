using Nimbus.DependencyResolution;
using Nimbus.Ninject.Configuration;
using Ninject;
using Ninject.Planning.Bindings.Resolvers;

namespace Nimbus.UnitTests.DependencyResolverTests.TestInfrastructure.DependencyResolverFactories
{
    public class NinjectDependencyResolverFactory : IDependencyResolverFactory
    {
        public IDependencyResolver Create(ITypeProvider typeProvider)
        {
            var kernel = new StandardKernel();
            kernel.Components.Remove<IMissingBindingResolver, SelfBindingResolver>();
            kernel.RegisterNimbus(typeProvider);
            return kernel.Get<IDependencyResolver>();
        }
    }
}