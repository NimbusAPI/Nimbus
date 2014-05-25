using System.Linq;
using Nimbus.DependencyResolution;
using Nimbus.Extensions;
using Nimbus.Ninject.Infrastructure;
using Ninject;

// ReSharper disable CheckNamespace
namespace Nimbus.Configuration
// ReSharper restore CheckNamespace
{
	public static class NimbusContainerBuilderExtensions
	{
		public static IKernel RegisterNimbus(this IKernel kernel, ITypeProvider typeProvider)
		{
			kernel.Bind<IDependencyResolver>().To<NinjectDependencyResolver>().InSingletonScope();
			kernel.Bind<ITypeProvider>().ToConstant(typeProvider).InSingletonScope();

			foreach (var handlerType in typeProvider.AllHandlerTypes())
			{
				var handlerInterfaceTypes = handlerType.GetInterfaces().Where(typeProvider.IsClosedGenericHandlerInterface);
				foreach (var interfaceType in handlerInterfaceTypes)
				{
					kernel.Bind(interfaceType).To(handlerType).InSingletonScope().Named(handlerType.FullName);
				}
			}

			return kernel;
		}
	}
}