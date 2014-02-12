using System;
using System.Configuration;
using System.Reflection;
using System.Threading.Tasks;
using Nimbus;
using Nimbus.Configuration;
using Nimbus.Infrastructure;
using Pizza.Maker.Messages;
using Pizza.Ordering.Messages;

#pragma warning disable 4014

namespace Pizza.Ordering
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // This is how you tell Nimbus where to find all your message types and handlers.
            var typeProvider = new AssemblyScanningTypeProvider(Assembly.GetExecutingAssembly(), typeof (NewOrderRecieved).Assembly, typeof (OrderPizzaCommand).Assembly);

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

            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("Press 1 to get the current wait time.");
                Console.WriteLine("Press 2 to order a pizza.");
                Console.WriteLine("Press 3 to Quit.");
                var input = Console.ReadLine();

                switch (input)
                {
                    case "1":

                        HowLongDoesAPizzaTake(bus);

                        break;

                    case "2":

                        Console.WriteLine("What's the customer's name?");
                        var customerName = Console.ReadLine().Trim();

                        if (string.IsNullOrWhiteSpace(customerName))
                        {
                            Console.WriteLine("You need to enter a customer name.");
                            continue;
                        }

                        var command = new OrderPizzaCommand {CustomerName = customerName};

                        bus.Send(command);

                        Console.WriteLine("Pizza ordered for {0}", customerName);

                        break;

                    case "3":
                        bus.Stop();
                        return;

                    default:
                        continue;
                }
            }
        }

        public static async Task HowLongDoesAPizzaTake(Bus bus)
        {
            var response = await bus.Request(new HowLongDoPizzasTakeRequest(), TimeSpan.FromSeconds(10));

            Console.WriteLine("Pizzas take about {0} minutes", response.Minutes);
        }
    }
}