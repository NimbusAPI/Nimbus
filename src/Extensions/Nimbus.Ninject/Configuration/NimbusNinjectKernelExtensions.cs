using System.Linq;
using Nimbus.DependencyResolution;
using Nimbus.Extensions;
using Nimbus.Ninject.Infrastructure;
using Ninject.Syntax;

namespace Nimbus.Ninject.Configuration
{
    public static class NimbusNinjectKernelExtensions
    {
        public static IBindingRoot RegisterNimbus(this IBindingRoot kernel, ITypeProvider typeProvider)
        {
            kernel.Bind<IDependencyResolver>()
                  .To<NinjectDependencyResolver>()
                  .InSingletonScope();

            kernel.Bind<ITypeProvider>()
                  .ToConstant(typeProvider)
                  .InSingletonScope();

            typeProvider.AllResolvableTypes()
                        .ToList()
                        .ForEach(t => kernel.Bind(t)
                                            .To(t)
                                            .InTransientScope());

            return kernel;
        }
    }
}