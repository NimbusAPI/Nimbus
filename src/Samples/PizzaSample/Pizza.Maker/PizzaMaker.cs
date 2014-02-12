using System;
using System.Collections.Generic;
using Nimbus;
using Pizza.Maker.Messages;

namespace Pizza.Maker
{
    public class PizzaMaker : IPizzaMaker
    {
        private readonly IBus _bus;

        private readonly List<string> _currentPizzaOrders = new List<string>();

        public PizzaMaker(IBus bus)
        {
            _bus = bus;
        }

        public void TakePizzaOrder(string customerName)
        {
            if (_currentPizzaOrders.Contains(customerName)) return;

            _currentPizzaOrders.Add(customerName);

            Console.WriteLine("Have got your order {0}", customerName);
        }

        public void CompletePizza(string customerName)
        {
            if (!_currentPizzaOrders.Contains(customerName)) return;

            _currentPizzaOrders.Remove(customerName);
            _bus.Publish(new PizzaIsReady {CustomerName = customerName});
        }
    }
}