using Autofac;
using AutofacSerilogIntegration;

namespace Barista.Modules
{
    public class LoggerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterLogger();
        }
    }
}