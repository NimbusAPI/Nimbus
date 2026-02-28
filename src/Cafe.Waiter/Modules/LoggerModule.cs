
using Autofac;
using AutofacSerilogIntegration;

namespace Waiter.Modules
{
    public class LoggerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {

            builder.RegisterLogger();
        }
    }
}