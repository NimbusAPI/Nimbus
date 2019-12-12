using System;
using Autofac;
using DevHarness.Harnesses;
using DevHarness.Messages;
using Serilog;

namespace DevHarness
{
    class Program
    {
        static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                         .MinimumLevel.Debug()
                         .WriteTo.Console()
                         .WriteTo.Seq("http://localhost:5341")
                         .CreateLogger();
            using (var container = IoC.LetThereBeIoC())
            {
                Console.WriteLine("Ready");

                var test = container.Resolve<CommandSender>();
                test.Run();
                
                Console.ReadKey();
                return 0;
            }

            return 1;
            
        }
    }
}
