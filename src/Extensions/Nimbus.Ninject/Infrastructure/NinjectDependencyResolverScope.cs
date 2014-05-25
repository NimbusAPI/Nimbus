using System;
using Nimbus.DependencyResolution;
using Ninject;
using Ninject.Activation.Blocks;

namespace Nimbus.Ninject.Infrastructure
{
	public class NinjectDependencyResolverScope : IDependencyResolverScope
	{
		private readonly IKernel _kernel;
		private readonly IActivationBlock _scope;

		public NinjectDependencyResolverScope(IKernel kernel)
		{
			_kernel = kernel;
			_scope = kernel.BeginBlock();
		}

		public IDependencyResolverScope CreateChildScope()
		{
			return new NinjectDependencyResolverScope(_kernel);
		}

		public void Dispose()
		{
			_scope.Dispose();
		}

		public TComponent Resolve<TComponent>(string componentName)
		{
			return _kernel.Get<TComponent>(componentName);
		}

		public object Resolve(Type componentType, string componentName)
		{
			return _kernel.Get(componentType, componentName);
		}
	}
}