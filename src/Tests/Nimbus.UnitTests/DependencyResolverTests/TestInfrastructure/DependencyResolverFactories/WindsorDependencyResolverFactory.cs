using Castle.Windsor;
using Nimbus.DependencyResolution;
using Nimbus.Windsor.Configuration;

namespace Nimbus.UnitTests.DependencyResolverTests.TestInfrastructure.DependencyResolverFactories
{
    public class WindsorDependencyResolverFactory : IDependencyResolverFactory
    {
        public IDependencyResolver Create(ITypeProvider typeProvider)
        {
            var container = new WindsorContainer();
            container.RegisterNimbus(typeProvider);
            return container.Resolve<IDependencyResolver>();
        }
    }
}