using System;
using System.Collections.Generic;
using System.Linq;

namespace Pizza.WaitTimeService
{
    public interface IWaitTimeCounter
    {
        void RecordNewPizzaOrder(int id);
        void RecordPizzaCompleted(int id);
        int GetAveragePizzaTimes();
    }

    public class WaitTimeCounter : IWaitTimeCounter
    {

        private Dictionary<int, PizzaTime> _pizzaTimes = new Dictionary<int, PizzaTime>();
 
        
        public void RecordNewPizzaOrder(int id)
        {
            if ( ! _pizzaTimes.ContainsKey(id))
            _pizzaTimes.Add(id, new PizzaTime{OrderRecieved = DateTime.Now});
            
        }

        public void RecordPizzaCompleted(int id)
        {
            if ( _pizzaTimes.ContainsKey(id))
                _pizzaTimes[id].PizzaCooked = DateTime.Now;
        }

        public int GetAveragePizzaTimes()
        {
            if (!_pizzaTimes.Any())
                return 10;

            var cookedPizzas = _pizzaTimes.Values.Where(pi => pi.PizzaCooked.HasValue);

            if (! cookedPizzas.Any())
                return 10;

            return (int)  cookedPizzas.Select(pi => (pi.PizzaCooked.Value - pi.OrderRecieved).TotalMinutes).Average();

        }

    }

    public class PizzaTime
    {
        public DateTime OrderRecieved { get; set; }
        public DateTime? PizzaCooked { get; set; }

    }


}