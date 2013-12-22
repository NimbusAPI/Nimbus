using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Nimbus;
using Nimbus.Configuration;
using Nimbus.Infrastructure;
using Pizza.Maker.Messages;
using Pizza.Ordering.Messages;

namespace Pizza.Ordering
{
    class Program
    {
        static void Main(string[] args)
        {

            // This is how you tell Nimbus where to find all your message types and handlers.
            var typeProvider = new AssemblyScanningTypeProvider(Assembly.GetExecutingAssembly(), typeof(NewOrderRecieved).Assembly, typeof(OrderPizzaCommand).Assembly);

            var messageBroker = new DefaultMessageBroker(typeProvider);

            var connectionString = ConfigurationManager.AppSettings["AzureConnectionString"];

            var bus = new BusBuilder().Configure()
                                        .WithNames("Ordering", Environment.MachineName)
                                        .WithConnectionString(connectionString)
                                        .WithTypesFrom(typeProvider)
                                        .WithCommandBroker(messageBroker)
                                        .WithRequestBroker(messageBroker)
                                        .WithMulticastEventBroker(messageBroker)
                                        .WithCompetingEventBroker(messageBroker)
                                        .WithMulticastRequestBroker(messageBroker)
                                        .WithDefaultTimeout(TimeSpan.FromSeconds(10))
                                        .Build();
            bus.Start();


            Console.WriteLine("Press 1 to get the current wait time.");
            Console.WriteLine("Press 2 to order a pizza.");
            Console.WriteLine("Press 3 to Quit.");


            int nextPizzaId = 1;

            while (true)
            {

                var input = Console.ReadLine();

                switch (input)
                {
                    case "1":

                        FindOutHowLongItWillBe(bus);

                        break;

                    case "2":

                        var command = new OrderPizzaCommand {PizzaId = nextPizzaId};

                        bus.Send(command);


                        Console.WriteLine("Pizza number {0} ordered", nextPizzaId);
                        nextPizzaId++;

                        break;
                    
                    case "3":
                        bus.Stop();
                        return;

                    default:
                        continue;
                        
                }


            }

        }

        public static async Task FindOutHowLongItWillBe(Bus bus)
        {
            var response = await bus.Request(new HowLongDoPizzasTakeRequest(), TimeSpan.FromSeconds(10));

            Console.WriteLine("Pizzas take about {0} minutes", response.Minutes);

        }
    }
}
