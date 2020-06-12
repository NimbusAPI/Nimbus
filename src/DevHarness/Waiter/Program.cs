﻿using System;
using Autofac;
using Serilog;

namespace Waiter
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                         .WriteTo.Console()
                         .MinimumLevel.Debug()
                         .WriteTo.Seq("http://localhost:5341")
                         .Enrich.WithProperty("Application", "Waiter")
                         .CreateLogger();

            var builder = new ContainerBuilder();
            builder.RegisterAssemblyModules(typeof(Program).Assembly);

            using (builder.Build())
            {
                Console.ReadKey();
                return;
            }
        }
    }
}
