using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Pizza.WaitTimeService
{
    public class WaitTimeCounter : IWaitTimeCounter
    {
        private readonly ConcurrentDictionary<int, PizzaTime> _pizzaTimes = new ConcurrentDictionary<int, PizzaTime>();

        public void RecordNewPizzaOrder(int id)
        {
            _pizzaTimes[id] = new PizzaTime {OrderRecieved = DateTime.Now};
        }

        public void RecordPizzaCompleted(int id)
        {
            PizzaTime cookedPizzaTime;
            if (!_pizzaTimes.TryGetValue(id, out cookedPizzaTime)) return;

            cookedPizzaTime.PizzaCooked = DateTime.Now;
        }

        public int GetAveragePizzaTimes()
        {
            if (!_pizzaTimes.Any())
                return 10;

            var cookedPizzas = _pizzaTimes.Values.Where(pi => pi.PizzaCooked.HasValue).ToArray();

            if (! cookedPizzas.Any())
                return 10;

            return (int) cookedPizzas.Select(pi => (pi.PizzaCooked.Value - pi.OrderRecieved).TotalMinutes).Average();
        }
    }
}