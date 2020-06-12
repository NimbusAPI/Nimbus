using Autofac;
using Waiter.Services;

namespace Waiter.Modules
{
    public class ServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<OrderDeliveryService>()
                .AsImplementedInterfaces()
                .SingleInstance();
        }
    }
}