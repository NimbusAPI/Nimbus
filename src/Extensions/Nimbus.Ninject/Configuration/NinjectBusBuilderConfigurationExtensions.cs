using Nimbus.DependencyResolution;
using Ninject;

// ReSharper disable CheckNamespace
namespace Nimbus.Configuration
// ReSharper restore CheckNamespace
{
	public static class NinjectBusBuilderConfigurationExtensions
	{
		public static BusBuilderConfiguration WithNinjectDefaults(this BusBuilderConfiguration configuration, IKernel kernel)
		{
			return configuration
				.WithDependencyResolver(kernel.Get<IDependencyResolver>())
				.WithLogger(kernel.Get<ILogger>());
		}
	}
}