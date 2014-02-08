using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;

namespace Pizza.RetailWeb
{
    public static class ContainerConfig
    {
        public static void Configure()
        {
            var builder = new ContainerBuilder();
            builder.RegisterAssemblyModules(typeof (ContainerConfig).Assembly);
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}