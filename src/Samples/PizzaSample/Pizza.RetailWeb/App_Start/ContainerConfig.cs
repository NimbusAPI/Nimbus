using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;

namespace Pizza.RetailWeb
{
    public static class ContainerConfig
    {
        private static IContainer _container;

        public static void Configure()
        {
            var builder = new ContainerBuilder();
            builder.RegisterAssemblyModules(typeof (ContainerConfig).Assembly);
            _container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(_container));
        }

        public static void Teardown()
        {
            _container.Dispose();
        }
    }
}