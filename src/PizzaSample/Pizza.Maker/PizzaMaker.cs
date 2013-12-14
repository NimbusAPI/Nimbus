using System;
using System.Collections;
using System.Collections.Generic;
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

        private List<int> listOfPizzasToMake = new List<int>(); 
        public void TakePizzaOrder(int pizzaId)
        {
            if (listOfPizzasToMake.Contains(pizzaId))
                return;

            
            listOfPizzasToMake.Add(pizzaId);
            
            Console.WriteLine("Have got your order {0}", pizzaId);


        }

        public void CompletePizza(int pizzaId)
        {
            if (listOfPizzasToMake.Contains(pizzaId))
            {
                listOfPizzasToMake.Remove(pizzaId);

                _bus.Publish(new PizzaIsReady {PizzaId = pizzaId});
            }
        }
         
    }
}