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
            kernel.Bind<IDependencyResolver>().To<NinjectDependencyResolver>().InSingletonScope();
            kernel.Bind<ITypeProvider>().ToConstant(typeProvider).InSingletonScope();

            BindAllHandlerInterfaces(kernel, typeProvider);

            return kernel;
        }

        private static void BindAllHandlerInterfaces(IBindingRoot kernel, ITypeProvider typeProvider)
        {
            typeProvider.AllHandlerTypes()
                        .ToList()
                        .ForEach(
                            handlerType =>
                            handlerType.GetInterfaces()
                                       .Where(typeProvider.IsClosedGenericHandlerInterface)
                                       .ToList()
                                       .ForEach(interfaceType => kernel.Bind(interfaceType).To(handlerType).InTransientScope()));
        }
    }
}