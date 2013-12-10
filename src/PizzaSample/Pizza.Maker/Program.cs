using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Nimbus;
using Nimbus.Autofac.Configuration;
using Nimbus.Configuration;
using Nimbus.Infrastructure;
using Nimbus.Logger;
using Nimbus.Logger.Serilog;
using Pizza.Ordering.Messages;
using Serilog;

namespace Pizza.Maker
{
    class Program
    {
        static void Main(string[] args)
        {

            Log.Logger = new LoggerConfiguration()
                .WriteTo.ColoredConsole()
                .MinimumLevel.Debug()
                .CreateLogger();


            var builder = new ContainerBuilder();

            builder.RegisterType<PizzaMaker>().AsImplementedInterfaces().SingleInstance();

            SetUpBus(builder);
            var container = builder.Build();


            Console.WriteLine("PIZZA CHEF");
            Console.WriteLine("Enter a pizza ID when you're done cooking it. Anything else to enter");

            while (true)
            {
                var input = Console.ReadLine();
                int pizzaId = 0;

                if (int.TryParse(input, out pizzaId))
                {
                    var maker = container.Resolve<IPizzaMaker>();
                    maker.CompletePizza(pizzaId);
                }
                else
                {
                    return;
                }


            }


        }



        private static void SetUpBus(ContainerBuilder builder)
        {
            //TODO: Set up your own connection string in app.config
            var connectionString = ConfigurationManager.AppSettings["AzureConnectionString"];

            // You'll want a logger. There's a ConsoleLogger and a NullLogger if you really don't care. You can roll your
            // own by implementing the ILogger interface if you want to hook it to an existing logging implementation.
            builder.RegisterType<SerilogStaticLogger>()
                .AsImplementedInterfaces()
                .SingleInstance();

            // This is how you tell Nimbus where to find all your message types and handlers.
            var typeProvider = new AssemblyScanningTypeProvider(Assembly.GetExecutingAssembly(), typeof(OrderPizzaCommand).Assembly);

            builder.RegisterNimbus(typeProvider);
            builder.Register(componentContext => new BusBuilder()
                .Configure()
                .WithConnectionString(connectionString)
                .WithNames("Maker", Environment.MachineName)
                .WithTypesFrom(typeProvider)
                .WithAutofacDefaults(componentContext)
                .Build())
                .As<IBus>()
                .AutoActivate()
                .OnActivated(c => c.Instance.Start())
                .SingleInstance();
        }
    }
}
