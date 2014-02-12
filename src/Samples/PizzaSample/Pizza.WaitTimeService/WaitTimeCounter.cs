using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Pizza.WaitTimeService
{
    public class WaitTimeCounter : IWaitTimeCounter
    {
        private readonly ConcurrentDictionary<string, PizzaTime> _pizzaTimes = new ConcurrentDictionary<string, PizzaTime>();

        public void RecordNewPizzaOrder(string customerName)
        {
            _pizzaTimes[customerName] = new PizzaTime { OrderRecieved = DateTime.Now };
        }

        public void RecordPizzaCompleted(string customerName)
        {
            PizzaTime cookedPizzaTime;
            if (!_pizzaTimes.TryGetValue(customerName, out cookedPizzaTime)) return;

            cookedPizzaTime.PizzaCooked = DateTime.Now;
        }

        public int GetAveragePizzaTimes()
        {
            if (!_pizzaTimes.Any()) return 10;

            var cookedPizzas = _pizzaTimes.Values.Where(pi => pi.PizzaCooked.HasValue).ToArray();

            if (! cookedPizzas.Any())
                return 10;

            return (int) cookedPizzas.Select(pi => (pi.PizzaCooked.Value - pi.OrderRecieved).TotalMinutes).Average();
        }
    }
}