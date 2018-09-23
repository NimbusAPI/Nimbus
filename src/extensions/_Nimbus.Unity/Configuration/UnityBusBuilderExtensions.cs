using System.Reflection;
using Microsoft.Practices.Unity;
using Nimbus.Configuration;
using Nimbus.DependencyResolution;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.Logging;
using Nimbus.Unity.Infastructure;

namespace Nimbus.Unity.Configuration
{
    public static class UnityBusBuilderExtensions
    {
        /// <summary>
        ///     Setups up the service bus with a new Unity Container and AssemblyScanningTypeProvider, then registers the types
        ///     with the container.
        /// </summary>
        public static BusBuilderConfiguration WithUnityDependencyResolver(this BusBuilderConfiguration configuration)
        {
            return configuration.WithUnityDependencyResolver(new AssemblyScanningTypeProvider(Assembly.GetCallingAssembly()), new UnityContainer());
        }

        /// <summary>
        ///     Setups up the service bus with the specified Unity Container and AssemblyScanningTypeProvider, then registers the types
        ///     with the container.
        /// </summary>
        public static BusBuilderConfiguration WithUnityDependencyResolver(this BusBuilderConfiguration configuration, IUnityContainer unityContainer)
        {
            return configuration.WithUnityDependencyResolver(new AssemblyScanningTypeProvider(Assembly.GetCallingAssembly()), unityContainer);
        }

        /// <summary>
        ///     Setups up the service bus with a new Unity Container and registers the types found in the given type provider.
        /// </summary>
        public static BusBuilderConfiguration WithUnityDependencyResolver(this BusBuilderConfiguration configuration, ITypeProvider typeProvider)
        {
            return configuration.WithUnityDependencyResolver(typeProvider, new UnityContainer());
        }

        /// <summary>
        ///     Setups up the service bus with the given IUnityContainer and registers the types found in the given type provider.
        /// </summary>
        public static BusBuilderConfiguration WithUnityDependencyResolver(this BusBuilderConfiguration configuration, ITypeProvider typeProvider, IUnityContainer unityContainer)
        {
            var dependencyResolver = new UnityDependencyResolver(unityContainer);

            unityContainer.RegisterInstance<IDependencyResolver>(dependencyResolver);
            unityContainer.RegisterInstance<ITypeProvider>(typeProvider);

            foreach (var resolvedType in typeProvider.AllResolvableTypes())
            {
                unityContainer.RegisterType(resolvedType);
            }

            if (!unityContainer.IsRegistered<ILogger>())
            {
                unityContainer.RegisterInstance<ILogger>(new NullLogger());
            }

            return configuration.WithLogger(unityContainer.Resolve<ILogger>()).WithDependencyResolver(dependencyResolver);
        }
    }
}