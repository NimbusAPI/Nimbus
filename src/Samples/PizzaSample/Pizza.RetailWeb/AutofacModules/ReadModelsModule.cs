using Autofac;
using Pizza.RetailWeb.ReadModels;

namespace Pizza.RetailWeb.AutofacModules
{
    public class ReadModelsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterAssemblyTypes(ThisAssembly)
                   .Where(t => t.IsAssignableTo<IAmAReadModel>())
                   .AsImplementedInterfaces()
                   .AsSelf()
                   .SingleInstance();
        }
    }
}