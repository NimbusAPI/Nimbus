using System;
using Autofac;
using Serilog;

namespace Pizza.Maker
{
    internal class Program
    {
        private static void Main()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.ColoredConsole()
                .MinimumLevel.Debug()
                .CreateLogger();

            var builder = new ContainerBuilder();
            builder.RegisterType<PizzaMaker>()
                   .AsImplementedInterfaces()
                   .SingleInstance();
            builder.RegisterModule<BusModule>();

            using (builder.Build())
            {
                Log.Information("Pizza maker is online.");

                Console.WriteLine("Press a key to exit.");
                Console.ReadKey();
            }
        }
    }
}