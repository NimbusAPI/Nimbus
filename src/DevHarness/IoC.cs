using Autofac;

namespace DevHarness
{
    public class IoC
    {
        public static IContainer LetThereBeIoC()
        {
            var builder = new ContainerBuilder();
            builder.RegisterAssemblyModules(typeof(IoC).Assembly);
            return builder.Build();
        }
    }
}