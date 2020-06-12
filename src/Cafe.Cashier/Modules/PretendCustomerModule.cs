using Autofac;
using Cashier.Services;

namespace Cashier.Modules
{
    public class PretendCustomerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {

            builder.RegisterType<CustomerOrderGenerator>()
                   .AsSelf()
                   .SingleInstance()
                   .AutoActivate()
                   .OnActivated(c => c.Instance.Start());
        }
    }
}