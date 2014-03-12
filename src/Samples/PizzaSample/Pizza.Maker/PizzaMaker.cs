using System;
using System.Threading.Tasks;
using Nimbus;
using Pizza.Maker.Messages;

namespace Pizza.Maker
{
    public class PizzaMaker : IPizzaMaker
    {
        private readonly IBus _bus;

        public PizzaMaker(IBus bus)
        {
            _bus = bus;
        }

        public async Task MakePizzaForCustomer(string customerName)
        {
            await _bus.Publish(new NewOrderRecieved {CustomerName = customerName});
            Console.WriteLine("Have got your order {0}", customerName);

            await Task.Delay(TimeSpan.FromSeconds(45));

            await _bus.Publish(new PizzaIsReady {CustomerName = customerName});
            Console.WriteLine("Hey, {0}! Your pizza's ready!", customerName);
        }
    }
}