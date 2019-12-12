using Autofac;
using DevHarness.Harnesses;

namespace DevHarness.Modules
{
    public class HarnessModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<CommandSender>();
        }
    }
}